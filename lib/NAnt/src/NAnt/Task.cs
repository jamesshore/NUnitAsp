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
// Mike Krueger (mike@icsharpcode.net)
// Ian MacLean (ian_maclean@another.com)

using System;
using System.Reflection;
using System.Xml;
using SourceForge.NAnt.Attributes;

namespace SourceForge.NAnt {

    /// <summary>Provides the abstract base class for tasks.</summary>
    /// <remarks>A task is a piece of code that can be executed.</remarks>
    public abstract class Task : Element {
        Target _target = null;

        bool _failOnError = true;
        bool _verbose = false;
        bool _ifDefined = true;
        bool _unlessDefined = false;

        /// <summary>Determines if task failure stops the build, or is just reported. Default is "true".</summary>
        [TaskAttribute("failonerror")]
        [BooleanValidator()]
        public bool FailOnError {
            get { return _failOnError; }
            set { _failOnError = value; }
        }

        /// <summary>Task reports detailed build log messages.  Default is "false".</summary>
        [TaskAttribute("verbose")]
        [BooleanValidator()]
        public bool Verbose {
            get { return (_verbose || Project.Verbose); }
            set { _verbose = value; }
        }

        /// <summary>If true then the task will be executed; otherwise skipped. Default is "true".</summary>
        [TaskAttribute("if")]
        [BooleanValidator()]
        public bool IfDefined {
            get { return _ifDefined; }
            set { _ifDefined = value; }
        }

        /// <summary>Opposite of if.  If false then the task will be executed; otherwise skipped. Default is "false".</summary>
        [TaskAttribute("unless")]
        [BooleanValidator()]
        public bool UnlessDefined {
            get { return _unlessDefined; }
            set { _unlessDefined = value; }
        }

        /// <summary>The name of the task.</summary>
        public override string Name {
            get {
                string name = null;
                TaskNameAttribute taskName = (TaskNameAttribute) Attribute.GetCustomAttribute(GetType(), typeof(TaskNameAttribute));
                if (taskName != null) {
                    name = taskName.Name;
                }
                return name;
            }
        }

        /// <summary>The prefix used when sending messages to the log.</summary>
        public string LogPrefix {
            get {
                string prefix = "[" + Name + "] ";
                return prefix.PadLeft(Log.IndentSize);
            }
        }

        /// <summary>The target the task belongs to.</summary>
        public Target Target {
            get { return _target; }
            set { _target = value; }
        }

        /// <summary>Executes the task unless it is skipped.</summary>
        public void Execute() {
            if (IfDefined && !UnlessDefined) {
                try {
                    ExecuteTask();
                } catch (BuildException e) {
                    if (FailOnError) {
                        throw e;
                    } else {
                        Log.WriteLine(e.Message);
                        if (e.InnerException != null) {
                            Log.WriteLine(e.InnerException.Message);
                        }
                    }
                }
            }
        }

        /// <summary><note>Deprecated (to be deleted).</note></summary>
        protected override void InitializeElement(XmlNode elementNode) {
            // Just defer for now so that everything just works
            InitializeTask(elementNode);
        }

        /// <summary>Initializes the task.</summary>
        protected virtual void InitializeTask(XmlNode taskNode) {
        }

        /// <summary>Executes the task.</summary>
        protected abstract void ExecuteTask();
    }
}
