using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class SendPortsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPortsCheck(Environment environment) : base(environment)
        { }

        public List<Remediation> Execute()
        {
            throw new NotImplementedException();
        }

        private List<SendPort> ListSendPorts()
        {
            try
            {
                return BtsCatExplorer.SendPorts.Cast<SendPort>().Where(sendPort => !sendPort.IsDynamic).ToList();
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
                var sendPort = BtsCatExplorer.SendPorts[sendPortName];
                sendPort.Status = PortStatus.Started;
                BtsCatExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                BtsCatExplorer.DiscardChanges();
                return false;
            }
        }

        private bool UnenlistSendPort(string sSendPortName)
        {
            try
            {
                var sp = BtsCatExplorer.SendPorts[sSendPortName];
                sp.Status = PortStatus.Bound;

                BtsCatExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                BtsCatExplorer.DiscardChanges();
                return false;
            }
        }
    }
}