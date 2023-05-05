﻿using Newtonsoft.Json.Bson;
using Revit.Async;
using RevitAddin.CommandLoader.Extensions;
using RevitAddin.CommandLoader.Revit;
using RevitAddin.CommandLoader.Services;
using RevitAddin.CommandLoader.Views;
using ricaun.Revit.Mvvm;
using ricaun.Revit.UI;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RevitAddin.CommandLoader.ViewModels
{
    public class CompileViewModel : ObservableObject
    {
        #region Public Static
        public static CompileViewModel ViewModel { get; set; } = new CompileViewModel();
        #endregion

        #region Public Properties
        public string Text { get; set; } = CodeSamples.Command;
        public bool EnableText { get; set; } = true;
        public IAsyncRelayCommand Command => new AsyncRelayCommand(CompileText);
        #endregion

        #region Constructor
        public CompileViewModel()
        {

        }
        #endregion

        #region View / Window
        public string Title { get; set; } = AppName.GetNameVersion();
        public object Icon { get; set; } = Properties.Resources.CommandLoader.GetBitmapSource();
        public CompileView Window { get; private set; }
        public void Show()
        {
            if (Window is null)
            {
                Window = new CompileView();
                Window.DataContext = this;
                Window.SetAutodeskOwner();
                Window.Closed += (s, e) => { Window = null; };
                InitializeCompile();
            }
            Window?.Show();
            Window?.Activate();
        }
        #endregion

        #region Private Methods
        private async Task CompileText()
        {
            EnableText = false;

            var sources = new[] { Text };

            if (GistGithubUtils.TryGetGistString(Text, out string gistOutput))
            {
                sources = new[] { gistOutput };
            }
            if (GistGithubUtils.TryGetGistFilesContent(Text, out string[] gistContents))
            {
                sources = gistContents;
            }

            try
            {
                await RevitTask.RunAsync(() =>
                {
                    try
                    {
                        var assembly = new CodeDomService().GenerateCode(sources);
                        App.CreateCommands(assembly);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.ToString());
                    }
                });
            }
            finally
            {
                EnableText = true;
            }
        }

        private void InitializeCompile()
        {
            Task.Run(() =>
            {
                new CodeDomService().GenerateCode(CodeSamples.Command);
            });
        }
        #endregion
    }
}