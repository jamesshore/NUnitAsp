// NAnt - A .NET build tool
// Copyright (C) 2001-2002 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

/*
Examples:
"**\*.class" matches all .class files/dirs in a directory tree.

"test\a??.java" matches all files/dirs which start with an 'a', then two
more characters and then ".java", in a directory called test.

"**" matches everything in a directory tree.

"**\test\**\XYZ*" matches all files/dirs that start with "XYZ" and where
there is a parent directory called test (e.g. "abc\test\def\ghi\XYZ123").

Example of usage:

DirectoryScanner scanner = DirectoryScanner();
scanner.Includes.Add("**\\*.class");
scanner.Exlucdes.Add("modules\\*\\**");
scanner.BaseDirectory = "test";
scanner.Scan();
foreach (string filename in GetIncludedFiles()) {
    Console.WriteLine(filename);
}
*/

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceForge.NAnt {
    /// <summary>Used for searching file system based on given include/exclude rules.</summary>
    /// <example>
    ///     <para>Simple client code for testing the class.</para>
    ///     <code>
    ///         while(true) {
    ///             DirectoryScanner scanner = new DirectoryScanner();
    ///    			
    ///             Console.Write("Scan Basedirectory : ");
    ///             string s = Console.ReadLine();
    ///             if (s == "") break;
    ///             scanner.BaseDirectory = s;
    ///
    ///             while(true) {
    ///                 Console.Write("Include pattern : ");
    ///                 s = Console.ReadLine();
    ///                 if (s == "") break;
    ///                 scanner.Includes.Add(s);
    ///             }
    ///
    ///             while(true) {
    ///                 Console.Write("Exclude pattern : ");
    ///                 s = Console.ReadLine();
    ///                 if (s == "") break;
    ///                 scanner.Excludes.Add(s);
    ///             }
    ///				
    ///             foreach (string name in scanner.FileNames)
    ///                 Console.WriteLine("file:" + name);
    ///             foreach (string name in scanner.DirectoryNames)
    ///                 Console.WriteLine("dir :" + name);
    ///
    ///             Console.WriteLine("");
    ///         }
    ///     </code>
    /// </example>
    /// <history>
    ///     <change date="20020220" author="Ari H�nnik�inen">Added support for absolute paths and relative paths refering to parent directories ( ../ )</change>
    ///     <change date="20020221" author="Ari H�nnik�inen">Changed implementation because of performance reasons - now scanning each directory only once</change>
    /// </history>
    public class DirectoryScanner {

        // Set to current directory in Scan if user doesn't specify something first.
        // Keeping it null, lets the user detect if it's been set or not.
        string _baseDirectory = null; 

        // holds the nant patterns (absolute or relative paths)
        StringCollection _includes = new StringCollection();
        StringCollection _excludes = new StringCollection();

        // holds the nant patterns converted to regular expression patterns (absolute canonized paths)
        StringCollection _includePatterns = null;
        StringCollection _excludePatterns = null;

        // holds the result from a scan
        StringCollection _fileNames = null;
        StringCollection _directoryNames = null;

        // directories that should be scanned and directories scanned so far
        StringCollection _searchDirectories = null;
        StringCollection _pathsAlreadySearched = null;

        public StringCollection Includes {
            get { return _includes; }
        }

        public StringCollection Excludes {
            get { return _excludes; }
        }

        public string BaseDirectory {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        public StringCollection FileNames {
            get {
                if (_fileNames == null) {
                    Scan();
                }
                return _fileNames;
            }
        }

        public StringCollection DirectoryNames {
            get {
                if (_directoryNames == null) {
                    Scan();
                }
                return _directoryNames;
            }
        }



        /// <summary>
        ///     Use Includes and Excludes search criteria (relative to Basedirectory or absolute) to search for filesystem objects
        /// </summary>
        /// <history>
        ///     <change date="20020220" author="Ari H�nnik�inen">Totally changed the scanning strategy</change>
        ///     <change date="20020221" author="Ari H�nnik�inen">Changed it again because of performance reasons</change>
        /// </history>
        public void Scan() {
            if (BaseDirectory == null) {
                BaseDirectory = Environment.CurrentDirectory;
            }
    
            _includePatterns = new StringCollection();
            _excludePatterns = new StringCollection();
            _fileNames = new StringCollection();
            _directoryNames = new StringCollection();
            _searchDirectories = new StringCollection();
            _pathsAlreadySearched = new StringCollection();
            
            // convert given NAnt patterns to regex patterns with absolute paths
            // side effect: searchDirectories will be populated
            convertPatterns(_includes, _includePatterns);
            convertPatterns(_excludes, _excludePatterns);
            
            foreach (string path in _searchDirectories) {
                ScanDirectory(path);
            }
        }



        /// <summary>
        ///     Parses given NAnt search patterns for search directories and corresponding regex patterns
        /// </summary>
        /// <param name="nantPatterns">In. NAnt patterns. Absolute or relative paths.</param>
        /// <param name="regexPatterns">Out. Regex patterns. Absolute canonical paths.</param>
        /// <history>
        ///     <change date="20020221" author="Ari H�nnik�inen">Created</change>
        /// </history>
        public void convertPatterns(StringCollection nantPatterns, StringCollection regexPatterns) {
            string searchDirectory;
            string regexPattern;
            foreach (string nantPattern in nantPatterns) {
                parseSearchDirectoryAndPattern(nantPattern, out searchDirectory, out regexPattern);
                if (!_searchDirectories.Contains(searchDirectory)) {
                    _searchDirectories.Add(searchDirectory);
                }
                if (!regexPatterns.Contains(regexPattern))
                    regexPatterns.Add(regexPattern);
            }
        }


        
        /// <summary>
        ///     Given a NAnt search pattern returns a search directory and an regex search pattern.
        /// </summary>
        /// <param name="originalNAntPattern">NAnt searh pattern (relative to the Basedirectory OR absolute, relative paths refering to parent directories ( ../ ) also supported)</param>
        /// <param name="searchDirectory">Out. Absolute canonical path to the directory to be searched</param>
        /// <param name="regexPattern">Out. Regex search pattern (absolute canonical path)</param>
        /// <history>
        ///     <change date="20020220" author="Ari H�nnik�inen">Created</change>
        ///     <change date="20020221" author="Ari H�nnik�inen">Returning absolute regex patterns instead of relative nant patterns</change>
        /// </history>
        public void parseSearchDirectoryAndPattern(string originalNAntPattern, out string searchDirectory, out string regexPattern) {
            string s = originalNAntPattern;
            // search for the first wildcard character (if any) and exclude the rest of the string beginnning from the character
            char[] wildcards = {'?', '*'};
            int indexOfFirstWildcard = s.IndexOfAny(wildcards);
            if (indexOfFirstWildcard != -1) { // if found any wildcard characters
                s = s.Substring(0, indexOfFirstWildcard);
            }
            
            // find the last DirectorySeparatorChar (if any) and exclude the rest of the string
            int indexOfLastDirectorySeparator = s.LastIndexOf(Path.DirectorySeparatorChar);

            // substring preceding the separator represents our search directory and the part following it represents nant search pattern relative to it
            
            if (indexOfLastDirectorySeparator != -1) {
                s = originalNAntPattern.Substring(0, indexOfLastDirectorySeparator);
            } else {
                s = "";
            }
            // combine the relative path with the base directory and canonize the resulting path
            searchDirectory = new DirectoryInfo(Path.Combine(BaseDirectory, s)).FullName;
            
            string modifiedNAntPattern = originalNAntPattern.Substring(indexOfLastDirectorySeparator + 1);
            regexPattern = ToRegexPattern(searchDirectory, modifiedNAntPattern);
        }


        /// <summary>
        ///     Searches a directory recursively for files and directories matching the search criteria
        /// </summary>
        /// <param name="path">Directory in which to search (absolute canonical path)</param>
        /// <history>
        ///     <change date="20020221" author="Ari H�nnik�inen">Checking if the directory has already been scanned</change>
        /// </history>
        void ScanDirectory(string path) {

            // scan each directory only once
            if (_pathsAlreadySearched.Contains(path)) {
                return;
            }
            _pathsAlreadySearched.Add(path);

            // get info for the current directory
            DirectoryInfo currentDirectoryInfo = new DirectoryInfo(path);

            // scan subfolders
            foreach (DirectoryInfo directoryInfo in currentDirectoryInfo.GetDirectories()) {
                ScanDirectory(directoryInfo.FullName);
            }

            // scan files
            foreach (FileInfo fileInfo in currentDirectoryInfo.GetFiles()) {
                string filename = Path.Combine(path, fileInfo.Name);
                if (IsPathIncluded(filename)) {
                    _fileNames.Add(filename);
                }
            }

            // Check current path last so that delete task will correctly
            // delete empty directories.  This may *seem* like a special case
            // but it is more like formalizing something in a way that makes
            // writing the delete task easier :)
            if (IsPathIncluded(path)) {
                _directoryNames.Add(path);
            }
        }



        bool IsPathIncluded(string path) {
            bool included = false;

            // check path against includes
            foreach (string pattern in _includePatterns) {
                Match m = Regex.Match(path, pattern);
                if (m.Success) {
                    included = true;
                    break;
                }
            }

            // check path against excludes
            if (included) {
                foreach (string pattern in _excludePatterns) {
                    Match m = Regex.Match(path, pattern);
                    if (m.Success) {
                        included = false;
                        break;
                    }
                }
            }

            return included;
        }


        
        /// <summary>
        ///     Converts NAnt search pattern to a regular expression pattern
        /// </summary>
        /// <param name="baseDir">Base directory for the search</param>
        /// <param name="nantPattern">Search pattern relative to the search directory</param>
        /// <returns>Regular expresssion (absolute path) for searching matching file/directory names</returns>
        /// <history>
        ///     <change date="20020220" author="Ari H�nnik�inen">Added parameter baseDir, using  it instead of class member variable</change>
        /// </history>
        static string ToRegexPattern(string baseDir, string nantPattern) {

            StringBuilder pattern = new StringBuilder(nantPattern);

            // NAnt patterns can use either / \ as a directory seperator.
            // We must replace both of these characters with Path.DirectorySeperatorChar
            pattern.Replace('/',  Path.DirectorySeparatorChar);
            pattern.Replace('\\', Path.DirectorySeparatorChar);

            // Patterns MUST be full paths.
            if (!Path.IsPathRooted(pattern.ToString())) {
                pattern = new StringBuilder(Path.Combine(baseDir, pattern.ToString()));
            }

            // The '\' character is a special character in regular expressions
            // and must be escaped before doing anything else.
            pattern.Replace(@"\", @"\\");

            // Escape the rest of the regular expression special characters.
            // NOTE: Characters other than . $ ^ { [ ( | ) * + ? \ match themselves.
            // TODO: Decide if ] and } are missing from this list, the above
            // list of characters was taking from the .NET SDK docs.
            pattern.Replace(".", @"\.");
            pattern.Replace("$", @"\$");
            pattern.Replace("^", @"\^");
            pattern.Replace("{", @"\{");
            pattern.Replace("[", @"\[");
            pattern.Replace("(", @"\(");
            pattern.Replace(")", @"\)");
            pattern.Replace("+", @"\+");

            // Special case directory seperator string under Windows.
            string seperator = Path.DirectorySeparatorChar.ToString();
            if (seperator == @"\") {
                seperator = @"\\";
            }

            // Convert NAnt pattern characters to regular expression patterns.

            // SPECIAL CASE: to match subdirectory OR current directory.  If
            // we don't do this then we can write something like 'src/**/*.cs'
            // to match all the files ending in .cs in the src directory OR
            // subdirectories of src.
            pattern.Replace(seperator + "**", "(" + seperator + ".|)|");

            // | is a place holder for * to prevent it from being replaced in next line
            pattern.Replace("**", ".|");
            pattern.Replace("*", "[^" + seperator + "]*");
            pattern.Replace("?", "[^" + seperator + "]?");
            pattern.Replace('|', '*'); // replace place holder string

            // Help speed up the search
            pattern.Insert(0, '^'); // start of line
            pattern.Append('$'); // end of line

            return pattern.ToString();
        }
    }
}
