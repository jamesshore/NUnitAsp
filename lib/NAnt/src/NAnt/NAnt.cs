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

// Gerry Shaw (gerry_shaw@yahoo.com)
// Scott Hernandez (ScottHernandez@hotmail.com)

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Xsl;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SourceForge.NAnt {

    public class NAnt {
        public static int Main(string[] args) {
            try {
                Project project = null;

                const string buildfileOption   = "-buildfile:";
                const string setOption         = "-D:";
                const string helpOption        = "-help";
                const string projectHelpOption = "-projecthelp";
                const string verboseOption     = "-verbose";
                const string findOption        = "-find";
                //not documented. Used for testing.
                const string indentOption        = "-indent";

                bool showHelp = false;
                bool showProjectHelp = false;
                bool findInParent = false;
                bool verbose = false;
                System.Collections.Specialized.StringCollection targets = new System.Collections.Specialized.StringCollection();
                PropertyDictionary buildOptionProps = new PropertyDictionary();

                foreach (string arg in args) {
                    if (arg.StartsWith(indentOption)){
                        Log.IndentLevel = Int32.Parse(arg.Substring(indentOption.Length + 1));
                    } else if (arg.StartsWith(buildfileOption)) {
                        project = new Project(arg.Substring(buildfileOption.Length));
                    } else if (arg.StartsWith(setOption)) {
                        // Properties from command line cannot be overwritten by
                        // the build file.  Once set they are set for the rest of the build.
                        Match match = Regex.Match(arg, @"-D:(\w+.*)=(\w*.*)");
                        if (match.Success) {
                            string name = match.Groups[1].Value;
                            string value = match.Groups[2].Value;
                            buildOptionProps.AddReadOnly(name, value);
                        }
                    } else if (arg.StartsWith(helpOption)) {
                        showHelp = true;
                    } else if (arg.StartsWith(projectHelpOption)) {
                        showProjectHelp = true;
                    } else if (arg.StartsWith(verboseOption)) {
                        verbose = true;
                    } else if (arg.StartsWith(findOption)) {
                        findInParent = true;
                    } else if (arg.Length > 0) {
                        if (arg.StartsWith("-")) {
                            throw new ApplicationException(String.Format("Unknown argument '{0}'", arg));
                        }
                        // must be a target if not an option
                        targets.Add(arg);
                    }
                }

                if (showHelp) {
                    // Get version information directly from assembly.  This takes more
                    // work but keeps the version numbers being displayed in sync with
                    // what the assembly is marked with.
                    FileVersionInfo info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

                    const int optionPadding = 23;

                    Console.WriteLine("NAnt version {0} Copyright (C) 2001-{1} Gerry Shaw", 
                        info.FileMajorPart + "." + info.FileMinorPart + "." + info.FileBuildPart, 
                        DateTime.Now.Year);
                    Console.WriteLine("http://nant.sf.net");
                    Console.WriteLine();
                    Console.WriteLine("NAnt comes with ABSOLUTELY NO WARRANTY.");
                    Console.WriteLine("This is free software, and you are welcome to redistribute it under certain");
                    Console.WriteLine("conditions set out by the GNU General Public License.  A copy of the license");
                    Console.WriteLine("is available in the distribution package and from the NAnt web site.");
                    Console.WriteLine();
                    Console.WriteLine("usage: nant [options] [target [target2 [target3] ... ]]");
                    Console.WriteLine();
                    Console.WriteLine("options:");
                    Console.WriteLine("  {0} print this message", helpOption.PadRight(optionPadding));
                    Console.WriteLine("  {0} print project help information", projectHelpOption.PadRight(optionPadding));
                    Console.WriteLine("  {0} use given buildfile", (buildfileOption + "<file>").PadRight(optionPadding));
                    Console.WriteLine("  {0} search parent directories for buildfile", findOption.PadRight(optionPadding));
                    Console.WriteLine("  {0} use value for given property", (setOption + "<property>=<value>").PadRight(optionPadding));
                    Console.WriteLine("  {0} displays more information during build process", verboseOption.PadRight(optionPadding));
                    Console.WriteLine();
                    Console.WriteLine("A file ending in .build will be used if no buildfile is specified.");

                } else {
                    // Get build file name if the project has not been created. 
                    // If a build file was not specified on the command line.
                    if(project == null) {
                        project = new Project(GetBuildFileName(Environment.CurrentDirectory, null, findInParent));
                    }

                    project.Verbose = verbose;
                    string[] buildTargets = (string[])Array.CreateInstance(typeof(string), targets.Count);
                    targets.CopyTo(buildTargets,0);
                    project.BuildTargets.AddRange(buildTargets);
                    foreach(System.Collections.DictionaryEntry de in buildOptionProps) {
                        project.Properties.AddReadOnly((string)de.Key, (string)de.Value);
                    }

                    if (showProjectHelp) {
                        ShowProjectHelp(project.Doc);
                    } else {
                        if (!project.Run()) {
                            throw new ApplicationException("");
                        }
                    }
                    }
                return 0;

            } catch (ApplicationException e) {
                if (e.Message.Length > 0) {
                    Console.WriteLine(e.Message);
                }
                Console.WriteLine("Try 'nant -help' for more information");
                return 1;

            } catch (Exception e) {
                // all other exceptions should have been caught
                Console.WriteLine("INTERNAL ERROR");
                Console.WriteLine(e.ToString());
                Console.WriteLine();
                Console.WriteLine("Please send bug report to nant-developers@lists.sourceforge.net");
                return 2;
            }
        }

        public static void ShowProjectHelp(XmlDocument buildDoc) {
            
            string resourceDirectory = 
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NAnt";
           
            // load our transform file out of the embedded resources
            Stream xsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nant.xslt.projecthelp.xslt");
            
            XslTransform transform = new XslTransform();           
            XmlTextReader reader = new XmlTextReader( xsltStream, XmlNodeType.Document, null );            
            transform.Load(reader);

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            XsltArgumentList arguments = new XsltArgumentList();

            // Do transform
            transform.Transform(buildDoc, arguments, writer );
            string outstr = sb.ToString();
            System.Console.WriteLine( sb.ToString() );
        }

        /// <summary>
        /// Gets the file name for the build file in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to look for a build file.  When in doubt use Environment.CurrentDirectory for directory.</param>
        /// <param name="searchPattern">Look for a build file with this pattern or name.  If null look for a file that matches the default build pattern (*.build).</param>
        /// <param name="findInParent">Whether or not to search the parent directories for a build file.</param>
        /// <returns>The path to the build file or <c>null</c> if no build file could be found.</returns>
        public static string GetBuildFileName(string directory, string searchPattern, bool findInParent) {
            string buildFileName = null;
            if (Path.IsPathRooted(searchPattern)) {
                buildFileName = searchPattern;
            } else {
                if (searchPattern == null) {
                    searchPattern = "*.build";
                }

                // find first file ending in .build
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                FileInfo[] files = directoryInfo.GetFiles(searchPattern);
                if (files.Length == 1) {
                    buildFileName = Path.Combine(directory, files[0].Name);
                } else if (files.Length == 0) {
                    DirectoryInfo parentDirectoryInfo = directoryInfo.Parent;
                    if (findInParent && parentDirectoryInfo != null) {
                        buildFileName = GetBuildFileName(parentDirectoryInfo.FullName, searchPattern, findInParent);
                    } else {
                        throw new ApplicationException((String.Format("Could not find a '{0}' file in '{1}'", searchPattern, directory)));
                    }
                } else { // files.Length > 1
                    throw new ApplicationException(String.Format("More than one '{0}' file found in '{1}'.  Use -buildfile:<file> to specify.", searchPattern, directory));
                }
            }
            return buildFileName;
        }

    }
}
