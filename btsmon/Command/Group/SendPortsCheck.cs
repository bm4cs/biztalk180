using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class SendPortsCheck : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BtsCatalogExplorer _btsCatalogExplorer;
        private readonly Environment _environment;

        public SendPortsCheck(Environment environment)
        {
            _environment = environment;

            var connectionString =
                $"Integrated Security=SSPI;database={environment.MgmtDatabase};server={environment.GroupServer}" +
                (!string.IsNullOrEmpty(environment.GroupInstance) ? $"instance={environment.GroupInstance}" : "");

            Logger.Debug($"SendPortsCheck connection string '{connectionString}'");

            _btsCatalogExplorer = new BtsCatalogExplorer
            {
                ConnectionString = connectionString
            };
        }

        public List<Remediation> Execute()
        {
            throw new NotImplementedException();
        }

        private List<SendPort> ListSendPorts()
        {
            try
            {
                return _btsCatalogExplorer.SendPorts.Cast<SendPort>().Where(sendPort => !sendPort.IsDynamic).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        private bool StartSendPort(string sendPortName)
        {
            try
            {
                var sendPort = _btsCatalogExplorer.SendPorts[sendPortName];
                sendPort.Status = PortStatus.Started;
                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }

        private bool UnenlistSendPort(string sSendPortName)
        {
            try
            {
                var sp = _btsCatalogExplorer.SendPorts[sSendPortName];
                sp.Status = PortStatus.Bound;

                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }
    }
}