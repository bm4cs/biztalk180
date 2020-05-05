using System;
using System.Collections.Generic;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class OrchestrationsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BtsCatalogExplorer _btsCatalogExplorer;
        private readonly Environment _environment;

        public OrchestrationsCheck(Environment environment) : base(environment)
        { }

        public List<Remediation> Execute()
        {
            throw new NotImplementedException(); //TODO
        }

        private bool StartOrchestration(string sOrchestrationName)
        {
            var btsAssemblyCollection = _btsCatalogExplorer.Assemblies;

            foreach (BtsAssembly btsAssembly in btsAssemblyCollection)
                if (sOrchestrationName.Split(',')[1] == btsAssembly.DisplayName.Split(',')[0])
                    foreach (BtsOrchestration btsOrchestration in btsAssembly.Orchestrations)
                        if (sOrchestrationName == btsOrchestration.AssemblyQualifiedName)
                        {
                            btsOrchestration.Status = OrchestrationStatus.Started;
                            try
                            {
                                _btsCatalogExplorer.SaveChanges();
                                return true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                _btsCatalogExplorer.DiscardChanges();
                                return false;
                            }
                        }

            return false;
        }

        private bool UnenlistOrchestration(string sOrchestrationName)
        {
            var btsAssemblyCollection = _btsCatalogExplorer.Assemblies;

            foreach (BtsAssembly btsAssembly in btsAssemblyCollection)
                if (sOrchestrationName.Split(',')[1] == btsAssembly.DisplayName.Split(',')[0])
                    foreach (BtsOrchestration btsOrchestration in btsAssembly.Orchestrations)
                        if (sOrchestrationName == btsOrchestration.AssemblyQualifiedName)
                        {
                            btsOrchestration.AutoTerminateInstances = true;
                            btsOrchestration.Status = OrchestrationStatus.Unenlisted;
                            try
                            {
                                _btsCatalogExplorer.SaveChanges();
                                return true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                _btsCatalogExplorer.DiscardChanges();
                                return false;
                            }
                        }

            return false;
        }
    }
}