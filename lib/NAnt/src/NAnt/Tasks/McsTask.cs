// NAnt - A .NET build tool
// Copyright (C) 2002 Ximian, Inc.
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
// Martin Baulig (martin@gnome.org)
// Ian MacLean ( ian@maclean.ms )

using System;
using System.IO;
using SourceForge.NAnt.Attributes;

namespace SourceForge.NAnt.Tasks
{

    /// <summary>
    /// Compiles C# programs using the mono mcs compiler.
    /// </summary>
    /// <remarks>
    ///   <para>See the <a href="http://www.go-mono.com">mono home page</a> for more information.</para>
    /// </remarks>
    /// <example>
    ///   <para>Compile <c>helloworld.cs</c> to <c>helloworld.exe</c>.</para>
    ///   <code>
    ///     <![CDATA[
    /// <mcs target="exe" output="helloworld.exe" debug="true">
    ///     <sources>
    ///         <includes name="helloworld.cs"/>
    ///     </sources>
    /// </mcs>
    ///     ]]>
    ///   </code>
    /// </example>
    [TaskName("mcs")]
    public class McsTask : CompilerBase
    {
        protected override void WriteOption(TextWriter writer, string name) {
            if (name.Equals("nologo")) {
                return;
            } else {
                writer.WriteLine("--{0}", name);
            }
        }

        protected override void WriteOption(TextWriter writer, string name, string arg) {
            if (name.Equals("out")) {
                writer.WriteLine("-o {0}", arg);
            } else if (name.Equals("reference")) {
                writer.WriteLine("-r {0}", arg);
            } else {
                writer.WriteLine("--{0} {1}", name, arg);
            }
        }
    }
}