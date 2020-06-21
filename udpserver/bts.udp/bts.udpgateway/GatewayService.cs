using bts.udpgateway.integration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;

namespace bts.udpgateway
{
    public partial class GatewayService : ServiceBase
    {
        #region Component Designer generated code

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // GatewayService
            // 
            this.CanShutdown = true;
            this.ServiceName = "BTS.UDPService";

        }

        #endregion

        public GatewayService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //LogHelper.Write("[START:BEGIN] Starting service...", "GatewayService.OnStart");
                if (!UDPProcess.IsRunning)
                {
                    UDPProcess.Start();
                    //LogHelper.Write("[START:RESULT] Service is started", "GatewayService.OnStart");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Write(ex, "GatewayService.OnStart");
                knote.utils.ShutdownSignal.Set();
            }
            finally
            {
                //LogHelper.Write("[START:END] Starting service.", "GatewayService.OnStart");
                if (!UDPProcess.IsRunning)
                    knote.utils.ShutdownSignal.Set();
            }
        }

        protected override void OnStop()
        {
            try
            {
                //LogHelper.Write("[STOP:BEGIN] Stopping service...", "GatewayService.OnStop");
                if (UDPProcess.IsRunning)
                {
                    UDPProcess.Stop();
                    //LogHelper.Write("[STOP:RESULT] Service is stopped", "GatewayService.OnStop");
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Write(ex, "GatewayService.OnStop");
                knote.utils.ShutdownSignal.Set();
            }
            finally
            {
                //LogHelper.Write("[STOP:END] Stopping service.", "GatewayService.OnStop");
                knote.utils.ShutdownSignal.Set();
            }
        }
    }
}
