using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using BTS = Microsoft.BizTalk.ExplorerOM;
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
            try
            {
                var remediationList = new List<Remediation>();

                foreach (BTS.BtsOrchestration orchestration in ListOrchestrations())
                {
                    var config = Environment.Applications?.SelectMany(a => a.Orchestrations)
                        .FirstOrDefault(p => p.Name.ToLower() == orchestration.FullName.Split(',')[0].ToLower());

                    if (config?.ExpectedState == ExpectedInstanceState.DontCare) continue;

                    String expectedState;
                    OrchestrationStatus newStatus;
                    String failText;
                    String actualState = orchestration.Status == OrchestrationStatus.Enlisted ? "Stopped"
                        : orchestration.Status == OrchestrationStatus.Unenlisted ? "Unenlisted"
                        : "Started";

                    if (config == null || config.ExpectedState == ExpectedInstanceState.Started)
                    {
                        expectedState = "Started";
                        newStatus = OrchestrationStatus.Started;
                        failText = $"Failed to start send port {orchestration.FullName}";
                    }
                    else
                    {
                        expectedState = "Unenlisted";
                        newStatus = OrchestrationStatus.Unenlisted;
                        failText = $"Failed to unenlist send port {orchestration.FullName}";
                    }

                    if (actualState != expectedState)
                    {
                        Remediation remediation = new Remediation
                        {
                            Name = orchestration.FullName,
                            Type = ArtifactType.Orchestration,
                            ExpectedState = expectedState,
                            ActualState = actualState,
                            RepairedTime = DateTime.Now,
                            Success = false
                        };

                        try
                        {
                            orchestration.Status = newStatus;
                            BtsCatExplorer.SaveChanges();
                            remediation.Success = true;
                        }
                        catch (Exception OrchestrationStartException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error(failText);
                            Logger.Error(OrchestrationStartException);
                        }
                        remediationList.Add(remediation);
                    }
                }

                return remediationList;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new List<Remediation>();
        }

        private IEnumerable<BTS.BtsOrchestration> ListOrchestrations()
        {
            try
            {
                List<BTS.BtsOrchestration> orchestrations = new List<BTS.BtsOrchestration>();
                foreach (BTS.Application app in BtsCatExplorer.Applications)
                {
                    foreach (BTS.BtsOrchestration orch in app.Orchestrations)
                    {
                        orchestrations.Add(orch);
                    }
                }

                return orchestrations;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
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

        //TODO: OH HELL NO!!!
        // We don't want to automatically terminate orchestration instances that should be stopped. That could be really bad. Better off suspending instead. 
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