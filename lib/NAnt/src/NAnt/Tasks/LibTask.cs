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
// Shawn Van Ness (nantluver@arithex.com)
// Gerry Shaw (gerry_shaw@yahoo.com)
// Ian MacLean ( ian@maclean.ms )
// Eric V. Smith (ericsmith@windsor.com)

// TODO: review interface for future compatibility/customizations issues

using System;
using System.IO;

using SourceForge.NAnt.Attributes;

namespace SourceForge.NAnt.Tasks {

    /// <summary>Run lib.exe, Microsoft's Library Manager.</summary>
    /// <example>
    ///   <para>Create a library.</para>
    ///   <code>
    /// <![CDATA[
    /// <lib output="library.lib">
    ///     <sources>
    ///         <includes name="library.obj"/>
    ///     </sources>
    /// </lib>
    /// ]]>
    ///   </code>
    /// </example>
    [TaskName("lib")]
    public class LibTask : ExternalProgramBase {
        string _responseFileName;

        string _output = null;
        FileSet _sources = new FileSet();
        FileSet _libdirs = new FileSet();
        string _options = null;

        /// <summary>
        /// Options to pass to the compiler.
        /// </summary>
        [TaskAttribute("options")]
        public string Options {get {return _options;} set {_options = value;}}

        /// <summary>
        /// The output file name.
        /// </summary>
        [TaskAttribute("output", Required=true)]
        public string Output {get {return _output;} set {_output = value;}}

        /// <summary>
        /// The list of files to combine into the output file.
        /// </summary>
        [FileSet("sources")]
        public FileSet Sources {get {return _sources;}}

        /// <summary>
        /// The list of additional library directories to search.
        /// </summary>
        [FileSet("libdirs")]
        public FileSet LibDirs {get {return _libdirs;}}

        // ExternalProgramBase implementation
        public override string ProgramFileName {get {return Name;}}
        public override string ProgramArguments {
            get {
                if (Verbose) {
                    return "@" + _responseFileName;
                } else {
                    return "/nologo @" + _responseFileName;
                }
            }
        }

        // Task implementation
        protected override void ExecuteTask() {
            if (Sources.BaseDirectory == null) {
                Sources.BaseDirectory = BaseDirectory;
            }

            Log.WriteLine(LogPrefix + "Combining {0} files to {1}", Sources.FileNames.Count, GetFullOutputPath());

            // Create temp response file to hold compiler options
            _responseFileName = Path.GetTempFileName();
            StreamWriter writer = new StreamWriter(_responseFileName);

            try {

                // specify the output file
                writer.WriteLine("/OUT:\"{0}\"", Path.Combine(BaseDirectory, Output));

                // write user provided options
                if (_options != null) {
                    writer.WriteLine(_options);
                }

                // write each of the filenames
                foreach(string filename in Sources.FileNames) {
                    writer.WriteLine(filename);
                }

                // write each of the libdirs
                foreach(string libdir in LibDirs.FileNames) {
                    writer.WriteLine("/LIBPATH:\"{0}\"", libdir);
                }

                writer.Close();

                // call base class to do the actual work
                base.ExecuteTask();
            } finally {
                // make sure we delete response file even if an exception is thrown
                writer.Close(); // make sure stream is closed or file cannot be deleted
                File.Delete(_responseFileName);
                _responseFileName = null;
            }
        }

        // Helper functions
        protected string GetFullOutputPath() {
            return Path.GetFullPath(Path.Combine(BaseDirectory, Output));
        }
    }
}
#if unused
Microsoft (R) Library Manager Version 7.00.9466
Copyright (C) Microsoft Corporation.  All rights reserved.

usage: LIB [options] [files]

   options:

      /DEF[:filename]
      /EXPORT:symbol
      /EXTRACT:membername
      /INCLUDE:symbol
      /LIBPATH:dir
      /LIST[:filename]
      /MACHINE:{AM33|ARM|IA64|M32R|MIPS|MIPS16|MIPSFPU|MIPSFPU16|MIPSR41XX|
                PPC|PPCFP|SH3|SH3DSP|SH4|SH5|THUMB|TRICORE|X86}
      /NAME:filename
      /NODEFAULTLIB[:library]
      /NOLOGO
      /OUT:filename
      /REMOVE:membername
      /SUBSYSTEM:{CONSOLE|EFI_APPLICATION|EFI_BOOT_SERVICE_DRIVER|
                  EFI_ROM|EFI_RUNTIME_DRIVER|NATIVE|POSIX|WINDOWS|
                  WINDOWSCE}[,#[.##]]
      /VERBOSE
#endif