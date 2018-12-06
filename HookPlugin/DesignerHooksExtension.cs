﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Napa;
using Napa.Common.Utils;
using Napa.Extensions;
using Napa.Scripting.Core;
using Napa.UI.Common.Dialogs;

namespace Napa.Hooks {

    /// <summary>
    /// Plugin for executing hooks scripts. Hook scripts are excuted when some event is raised.
    /// The scripts should be located to folder "Hooks" inside the same folder the plugin is placed.
    /// </summary>
    [Extension("DesignerHooks")]
    public class DesignerHooksExtension : IExtension {

        private IExtensionContext Context { get; set; }

        public void Initialize(IExtensionContext context) {
            Context = context;
            Context.ProjectEventSource.ProjectOpened += OnProjectOpened;
        }

        public void Dispose() {
            if (Context.CurrentProjectVersion == null) return;
            Context.CurrentProjectVersion.GeometryManager.GeometricObjectEntered -= OnObjectEntered;
        }

        private void OnObjectEntered(object o, EventArgs args) {
            var name = o.ToString();
            if (name.Contains("_TEMP_")) return;
            var surfaceObject = Context.CurrentProjectVersion.GeometryManager.GetSurfaceObject(name);
            if (surfaceObject == null) return;
            string scriptName = "";
            //TODO better way to check that object is just created
            var isNew = DateTime.Now - surfaceObject.Date < TimeSpan.FromSeconds(3);
            if (isNew) {
                try {
                    var path = Path.Combine(ProgramUtils.PathToAssembly(Assembly.GetExecutingAssembly()), "Hooks");
                    var scriptFiles = Directory.GetFiles(path, "*.cs");
                    foreach (var script in scriptFiles) {
                        scriptName = Path.GetFileName(script);
                        ScriptEngine.ExecuteFile(script, name);
                    }
                } catch(Exception e) {
                    ModalDialog.ShowException(e, "Hook error " + scriptName);
                }
            }
        }

        private void OnProjectOpened() {
            Context.CurrentProjectVersion.GeometryManager.GeometricObjectEntered += OnObjectEntered;
        }

    }
}