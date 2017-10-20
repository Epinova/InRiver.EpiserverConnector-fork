﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Epinova.InRiverConnector.EpiserverAdapter.Helpers;
using Epinova.InRiverConnector.Interfaces;
using Epinova.InRiverConnector.Interfaces.Enums;
using inRiver.Integration.Logging;
using inRiver.Remoting.Log;
using inRiver.Remoting.Objects;

namespace Epinova.InRiverConnector.EpiserverAdapter.Communication
{
    public class EpiApi
    {
        private readonly EpiMappingHelper _mappingHelper;
        private readonly CatalogCodeGenerator _catalogCodeGenerator;
        private readonly HttpClientInvoker _httpClient; 

        public EpiApi(Configuration config, EpiMappingHelper mappingHelper, CatalogCodeGenerator catalogCodeGenerator)
        {
            _mappingHelper = mappingHelper;
            _catalogCodeGenerator = catalogCodeGenerator;
            _httpClient = new HttpClientInvoker(config);
        }

        internal void DeleteCatalog(int catalogId, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    _httpClient.Post(config.Endpoints.DeleteCatalog, catalogId);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to delete catalog with id: {catalogId}", exception);
                }
            }
        }

        internal void DeleteCatalogNode(int catalogNodeId, int catalogId, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    string catalogNode = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(catalogNodeId);
                    _httpClient.Post(config.Endpoints.DeleteCatalogNode, catalogNode);
                }
                catch (Exception ex)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to delete catalogNode with id: {catalogNodeId} for channel: {catalogId}", ex);
                }
            }
        }

        internal void DeleteSku(string skuId, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    _httpClient.Post(config.Endpoints.DeleteCatalogEntry, skuId);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to delete catalog entry based on entity id: {entityId}", exception);
                }
            }
        }

        internal void DeleteCatalogEntry(Entity entity, Configuration config)
        {
            string catalogEntryId = _catalogCodeGenerator.GetEpiserverCode(entity);

            lock (EpiLockObject.Instance)
            {
                try
                {
                    _httpClient.Post(config.Endpoints.DeleteCatalogEntry, catalogEntryId);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to delete catalog entry with catalog entry ID: {catalogEntryId}", exception);
                }
            }
        }

        internal void UpdateLinkEntityData(Entity linkEntity, Entity channel, Configuration config, int parentId)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    string channelName = BusinessHelper.GetDisplayNameFromEntity(channel, config, -1);

                    string parentEntryId = _catalogCodeGenerator.GetEpiserverCode(parentId);
                    string linkEntityIdString = _catalogCodeGenerator.GetEpiserverCode(linkEntity);

                    string dispName = linkEntity.EntityType.Id + '_' + BusinessHelper.GetDisplayNameFromEntity(linkEntity, config, -1).Replace(' ', '_');

                    LinkEntityUpdateData dataToSend = new LinkEntityUpdateData
                                                          {
                                                              ChannelName = channelName,
                                                              LinkEntityIdString = linkEntityIdString,
                                                              LinkEntryDisplayName = dispName,
                                                              ParentEntryId = parentEntryId
                                                          };

                    _httpClient.Post(config.Endpoints.UpdateLinkEntityData, dataToSend);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to update data for link entity with id:{linkEntity.Id}", exception);
                }
            }
        }

        internal List<string> GetLinkEntityAssociationsForEntity(string linkType, 
                                                                 int channelId, 
                                                                 Entity channelEntity, 
                                                                 Configuration config, 
                                                                 List<string> parentIds, 
                                                                 List<string> targetIds)
        {
            lock (EpiLockObject.Instance)
            {
                List<string> ids = new List<string>();
                try
                {
                    string channelName = BusinessHelper.GetDisplayNameFromEntity(channelEntity, config, -1);

                    for (int i = 0; i < targetIds.Count; i++)
                    {
                        targetIds[i] = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(targetIds[i]);
                    }

                    for (int i = 0; i < parentIds.Count; i++)
                    {
                        parentIds[i] = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(parentIds[i]);
                    }

                    GetLinkEntityAssociationsForEntityData dataToSend = new GetLinkEntityAssociationsForEntityData
                                                                            {
                                                                                ChannelName = channelName,
                                                                                LinkTypeId = linkType,
                                                                                ParentIds = parentIds,
                                                                                TargetIds = targetIds
                                                                            };

                    ids = _httpClient.PostWithStringListAsReturn(config.Endpoints.GetLinkEntityAssociationsForEntity, dataToSend);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Warning, "Failed to get link entity associations for entity", exception);
                }

                return ids;
            }
        }

        internal void CheckAndMoveNodeIfNeeded(string nodeId, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    string entryNodeId = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(nodeId);
                    _httpClient.Post(config.Endpoints.CheckAndMoveNodeIfNeeded, entryNodeId);
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Warning, "Failed when calling the interface function: CheckAndMoveNodeIfNeeded", exception);
                }
            }
        }

        internal void UpdateEntryRelations(string catalogEntryId, 
                                                  int channelId,
                                                  Entity channelEntity,
                                                  Configuration config, 
                                                  string parentId,
                                                  Dictionary<string, bool> shouldExistInChannelNodes,
                                                  string linkTypeId, 
                                                  List<string> linkEntityIdsToRemove)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    string channelName = BusinessHelper.GetDisplayNameFromEntity(channelEntity, config, -1);
                    List<string> removeFromChannelNodes = new List<string>();
                    foreach (KeyValuePair<string, bool> shouldExistInChannelNode in shouldExistInChannelNodes)
                    {
                        if (!shouldExistInChannelNode.Value)
                        {
                            removeFromChannelNodes.Add(_catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(shouldExistInChannelNode.Key));
                        }
                    }

                    string parentEntryId = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(parentId);
                    string catalogEntryIdString = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(catalogEntryId);
                    string channelIdEpified = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(channelId);
                    bool relation = _mappingHelper.IsRelation(linkTypeId);
                    bool parentExistsInChannelNodes = shouldExistInChannelNodes.Keys.Contains(parentId);

                    var updateEntryRelationData = new UpdateRelationData
                                                {
                                                    ParentEntryId = parentEntryId,
                                                    CatalogEntryIdString = catalogEntryIdString,
                                                    ChannelIdEpified = channelIdEpified,
                                                    ChannelName = channelName,
                                                    RemoveFromChannelNodes = removeFromChannelNodes,
                                                    LinkEntityIdsToRemove = linkEntityIdsToRemove,
                                                    LinkTypeId = linkTypeId,
                                                    IsRelation = relation,
                                                    ParentExistsInChannelNodes = parentExistsInChannelNodes
                                                };

                    _httpClient.Post(config.Endpoints.UpdateEntryRelations, updateEntryRelationData);
                }
                catch (Exception exception)
                {
                    string parentEntryId = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(parentId);
                    string childEntryId = _catalogCodeGenerator.GetEpiserverCodeLEGACYDAMNIT(catalogEntryId);
                    IntegrationLogger.Write(
                        LogLevel.Error,
                        $"Failed to update entry relations between parent entry id {parentEntryId} and child entry id {childEntryId} in catalog with id {catalogEntryId}",
                        exception);
                }
            }
        }

        internal bool Import(string filePath, Guid guid, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    string result = _httpClient.Post(config.Endpoints.ImportCatalogXml, filePath);
                    IntegrationLogger.Write(LogLevel.Debug, $"Import catalog returned: {result}");
                    return true;
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to import catalog xml file {filePath}.", exception);
                    IntegrationLogger.Write(LogLevel.Error, exception.ToString());

                    return false;
                }
            }
        }

        internal bool ImportResources(string manifest, string baseFilePpath, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    var importer = new ResourceImporter(config);
                    importer.ImportResources(manifest, baseFilePpath);

                    IntegrationLogger.Write(LogLevel.Information, $"Resource file {manifest} imported to EPi Server Commerce.");
                    return true;
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error, $"Failed to import resource file {manifest}.", exception);
                    return false;
                }
            }
        }

        internal bool ImportUpdateCompleted(string catalogName, ImportUpdateCompletedEventType eventType, bool resourceIncluded, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    var data = new ImportUpdateCompletedData
                                {
                                    CatalogName = catalogName,
                                    EventType = eventType,
                                    ResourcesIncluded = resourceIncluded
                                };

                    string result = _httpClient.Post(config.Endpoints.ImportUpdateCompleted, data);
                    IntegrationLogger.Write(LogLevel.Debug, $"ImportUpdateCompleted returned: {result}");
                    return true;
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error,
                        $"Failed to fire import update completed for catalog {catalogName}.", exception);
                    return false;
                }
            }
        }

        internal bool DeleteCompleted(string catalogName, DeleteCompletedEventType eventType, Configuration config)
        {
            lock (EpiLockObject.Instance)
            {
                try
                {
                    var data = new DeleteCompletedData
                                {
                                   CatalogName = catalogName,
                                   EventType = eventType
                               };

                    string result = _httpClient.Post(config.Endpoints.DeleteCompleted, data);
                    IntegrationLogger.Write(LogLevel.Debug, $"DeleteCompleted returned: {result}");
                    return true;
                }
                catch (Exception exception)
                {
                    IntegrationLogger.Write(LogLevel.Error,
                        $"Failed to fire DeleteCompleted for catalog {catalogName}.", exception);
                    return false;
                }
            }
        }

        internal void SendHttpPost(Configuration config, string filepath)
        {
            if (string.IsNullOrEmpty(config.HttpPostUrl))
            {
                return;
            }

            try
            {
                string uri = config.HttpPostUrl;
                using (WebClient client = new WebClient())
                {
                    client.UploadFileAsync(new Uri(uri), "POST", @filepath);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogger.Write(LogLevel.Error, "Exception in SendHttpPost", ex);
            }
        }
    }
}
