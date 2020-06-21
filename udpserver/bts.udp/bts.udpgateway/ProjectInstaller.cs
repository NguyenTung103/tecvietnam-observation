using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace bts.udpgateway
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private IContainer components;

        private ServiceProcessInstaller _processInstaller;
        private ServiceInstaller _installer;

        public ProjectInstaller()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this._installer = new System.ServiceProcess.ServiceInstaller();
            // 
            // _processInstaller
            // 
            this._processInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this._processInstaller.Password = null;
            this._processInstaller.Username = null;
            // 
            // _installer
            // 
            this._installer.ServiceName = "BTS.UDPService";
            this._installer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this._processInstaller,
            this._installer});

        }
    }
}
