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
// Ian MacLean (ian_maclean@another.com)

using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

using SourceForge.NAnt.Attributes;

namespace SourceForge.NAnt.Tasks {

    /// <summary>Copies a file or fileset to a new file or directory.</summary>
    /// <remarks>
    ///   <para>Files are only copied if the source file is newer than the destination file, or if the destination file does not exist. However, you can explicitly overwrite files with the overwrite attribute.</para>
    ///   <para>Filesets are used to select files to copy. To use a fileset, the todir attribute must be set.</para>
    /// </remarks>
    /// <example>
    ///   <para>Copy a single file.</para>
    ///   <code>&lt;copy file="myfile.txt" tofile="mycopy.txt"/&gt;</code>
    ///   <para>Copy a set of files to a new directory.</para>
    ///   <code>
    /// <![CDATA[
    /// <copy todir="${build.dir}">
    ///     <fileset basedir="bin">
    ///         <includes name="*.dll"/>
    ///     </fileset>
    /// </copy>
    /// ]]>
    ///   </code>
    /// </example>
    [TaskName("copy")]
    public class CopyTask : Task {

        string _sourceFile = null;
        string _toFile = null;
        string _toDirectory = null;
        bool _overwrite = false;
        //bool _includeEmptyDirs = false;
        //bool _preserveLastModified = false;
        FileSet _fileset = new FileSet();
        Hashtable _fileCopyMap = new Hashtable();

        /// <summary>The file to copy.</summary>
        [TaskAttribute("file")]
        public string SourceFile        { get { return _sourceFile; } set {_sourceFile = value; } }

        /// <summary>The file to copy to.</summary>
        [TaskAttribute("tofile")]
        public string ToFile            { get { return _toFile; } set {_toFile = value; } }

        /// <summary>The directory to copy to.</summary>
        [TaskAttribute("todir")]
        public string ToDirectory       { get { return _toDirectory; } set {_toDirectory = value; } }

        // /// <summary>Copy empty directories included with the nested fileset(s). Defaults to "true".</summary>
        //[TaskAttribute("includeEmptyDirs")]
        //[BooleanValidator()]
        //public bool IncludeEmptyDirs    { get { return (_includeEmptyDirs); } set {_includeEmptyDirs = value; } }

        /// <summary>Overwrite existing files even if the destination files are newer. Defaults to "false".</summary>
        [TaskAttribute("overwrite")]
        [BooleanValidator()]
        public bool Overwrite           { get { return (_overwrite); } set {_overwrite = value; } }

        // /// <summary>Give the copied files the same last modified time as the original files. Defaults to "false".</summary>
        // [TaskAttribute("preserveLastModified")]
        // [BooleanValidator()]
        // public bool PreserveLastModified{ get { return (_preserveLastModified); } set {_preserveLastModified = value; } }

        /// <summary>Filesets are used to select files to copy. To use a fileset, the todir attribute must be set.</summary>
        [FileSet("fileset")]
        public FileSet CopyFileSet      { get { return _fileset; } }

        protected Hashtable FileCopyMap {
            get { return _fileCopyMap; }
        }

        /// <summary>
        /// Actually does the file (and possibly empty directory) copies.
        /// </summary>
        protected virtual void DoFileOperations() {
            int fileCount = FileCopyMap.Keys.Count;
            if (fileCount > 0 || Verbose) {
                if (ToDirectory != null) {
                    Log.WriteLine(LogPrefix + "Copying {0} files to {1}", fileCount, Project.GetFullPath(ToDirectory));
                } else {
                    Log.WriteLine(LogPrefix + "Copying {0} files", fileCount);
                }

                // loop thru our file list
                foreach (string sourcePath in FileCopyMap.Keys) {
                    string dstPath = (string)FileCopyMap[sourcePath];
                    if (sourcePath == dstPath) {
                        Log.WriteLineIf(Verbose, LogPrefix + "Skipping self-copy of {0}" + sourcePath);
                        continue;
                    }

                    try {
                        Log.WriteLineIf(Verbose, LogPrefix + "Copying {0} to {1}", sourcePath, dstPath);

                        // create directory if not present
                        string dstDirectory = Path.GetDirectoryName(dstPath);
                        if (!Directory.Exists(dstDirectory)) {
                            Directory.CreateDirectory(dstDirectory);
                            Log.WriteLineIf(Verbose, LogPrefix + "Created directory {0}", dstDirectory);
                        }

                        File.Copy(sourcePath, dstPath, true);
                    } catch (IOException ioe) {
                        string msg = String.Format("Cannot copy {0} to {1}", sourcePath, dstPath);
                        throw new BuildException(msg, Location, ioe);
                    }
                }
            }

            // TODO: handle empty directories in the fileset, refer to includeEmptyDirs attribute at
            // http://jakarta.apache.org/ant/manual/CoreTasks/copy.html
        }

        protected override void ExecuteTask() {
            // NOTE: when working with file and directory names its useful to 
            // use the FileInfo an DirectoryInfo classes to normalize paths like:
            // c:\work\nant\extras\buildserver\..\..\..\bin

            if (SourceFile != null) {
                // Copy single file.

                FileInfo srcInfo = new FileInfo(Project.GetFullPath(SourceFile));
                if (srcInfo.Exists) {
                    FileInfo dstInfo = null;
                    if (ToFile != null) {
                        dstInfo = new FileInfo(Project.GetFullPath(ToFile));
                    } else {
                        string dstDirectoryPath = Project.GetFullPath(ToDirectory);
                        string dstFilePath = Path.Combine(dstDirectoryPath, srcInfo.Name);
                        dstInfo = new FileInfo(dstFilePath);

                    }

                    // do the outdated check
                    bool outdated = (!dstInfo.Exists) || (srcInfo.LastWriteTime > dstInfo.LastWriteTime);

                    if (Overwrite || outdated) {
                        // add to a copy map of absolute verified paths
                        FileCopyMap.Add(srcInfo.FullName, dstInfo.FullName);
                    }
                } else {
                    Log.WriteLine(LogPrefix + "Could not find file {0} to copy.", SourceFile);
                }
            } else {
                // Copy file set contents.

                // get the complete path of the base directory of the fileset, ie, c:\work\nant\src
                DirectoryInfo srcBaseInfo = new DirectoryInfo(CopyFileSet.BaseDirectory);
                DirectoryInfo dstBaseInfo = new DirectoryInfo(Project.GetFullPath(ToDirectory));

                // if source file not specified use fileset
                foreach (string pathname in CopyFileSet.FileNames) {
                    FileInfo srcInfo = new FileInfo(pathname);
                    if (srcInfo.Exists) {
                        // replace the fileset path with the destination path
                        // NOTE: big problems could occur if the file set base dir is rooted on a different drive
                        string dstPath = pathname.Replace(srcBaseInfo.FullName, dstBaseInfo.FullName);

                        // do the outdated check
                        FileInfo dstInfo = new FileInfo(dstPath);
                        bool outdated = (!dstInfo.Exists) || (srcInfo.LastWriteTime > dstInfo.LastWriteTime);

                        if (Overwrite || outdated) {
                            FileCopyMap.Add(pathname, dstPath);
                        }
                    } else {
                        Log.WriteLine(LogPrefix + "Could not find file {0} to copy.", pathname);
                    }
                }
            }

            // do all the actual copy operations now...
            DoFileOperations();
        }
    }
}
