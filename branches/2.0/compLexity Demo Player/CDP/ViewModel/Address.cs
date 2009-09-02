﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace CDP.ViewModel
{
    public class Address : Core.ViewModelBase
    {
        public string SelectedFolder
        {
            get { return selectedFolder; }
            set
            {
                lastSelectedFolder = selectedFolder;
                selectedFolder = value;

                if (lastSelectedFolder != selectedFolder)
                {
                    mediator.Notify<string>(Messages.SelectedFolderChanged, selectedFolder);
                }
            }
        }

        private IMediator mediator;
        private string lastSelectedFolder;
        private string selectedFolder;

        public Address()
            : this(Mediator.Instance)
        {
        }

        public Address(IMediator mediator)
        {
            this.mediator = mediator;
            selectedFolder = "";
        }

        public override void Initialise()
        {
            mediator.Register<string>(Messages.SetSelectedFolder, SetSelectedFolder, this);
        }

        public override void Initialise(object parameter)
        {
            throw new NotImplementedException();
        }

        public void SetSelectedFolder(string path)
        {
            SelectedFolder = path;
            OnPropertyChanged("SelectedFolder");
        }
    }
}
