// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
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

namespace SourceForge.NAnt.Tasks {

    using System;
    using SourceForge.NAnt.Attributes;

    /// <summary>Sets a property in the current project.</summary>
    /// <remarks>
    ///   <note>NAnt uses a number of predefined properties.</note>
    /// </remarks>
    /// <example>
    ///   <para>Define a <c>debug</c> property with the value <c>true</c>.</para>
    ///   <code><![CDATA[<property name="debug" value="true"/>]]></code>
    ///   <para>Use the user-defined <c>debug</c> property.</para>
    ///   <code><![CDATA[<property name="trace" value="${debug}"/>]]></code>
    /// </example>
    [TaskName("property")]
    public class PropertyTask : Task {
        
        string _name = null;        
        string _value = String.Empty;

         /// <summary>the name of the property to set.</summary>        
       [TaskAttribute("name", Required=true)]
       public string PropName { get { return _name; } set { _name = value; } }

         /// <summary>the value of the property.</summary>        
        [TaskAttribute("value", Required=true)]
        public string Value { get { return _value; } set { _value = value; } }
       
        protected override void ExecuteTask() {
            Project.Properties[_name] = _value;
        }
    }
}
