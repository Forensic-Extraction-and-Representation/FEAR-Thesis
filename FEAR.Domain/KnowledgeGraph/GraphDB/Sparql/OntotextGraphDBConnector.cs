#region Assembly dotNetRdf.Client, Version=3.3.2.0, Culture=neutral, PublicKeyToken=6055ffe4c97cc780
// location unknown
// Decompiled with ICSharpCode.Decompiler 8.2.0.7535
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Storage;
using VDS.RDF;

namespace FEAR.Domain.KnowledgeGraph.GraphDB.Sparql;

//
// Summary:
//     Class for connecting to any dataset that can be exposed via Fuseki.
//
// Remarks:
//     Uses all three Services provided by a Fuseki instance - Query, Update and HTTP
//     Update.
public class OntotextGraphDBConnector : SparqlHttpProtocolConnector, IAsyncUpdateableStorage, IAsyncQueryableStorage, IAsyncStorageProvider, IStorageCapabilities, IDisposable, IUpdateableStorage, IQueryableStorage, IStorageProvider
{
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    private readonly string _updateUri;

    private readonly string _queryUri;

    //
    // Summary:
    //     Returns that Listing Graphs is supported.
    public override bool ListGraphsSupported => true;

    //
    // Summary:
    //     Gets the IO Behaviour of the Store.
    public override IOBehaviour IOBehaviour => base.IOBehaviour | IOBehaviour.CanUpdateDeleteTriples;

    //
    // Summary:
    //     Returns that Triple level updates are supported using Fuseki.
    public override bool UpdateSupported => true;

    //
    // Summary:
    //     Creates a new connection to a Fuseki Server.
    //
    // Parameters:
    //   serviceUri:
    //     The /data URI of the Fuseki Server.
    //
    //   writerMimeTypeDefinition:
    //     The MIME type of the syntax to use when sending RDF data to the server. Defaults
    //     to RDF/XML.
    public OntotextGraphDBConnector(Uri serviceUri, MimeTypeDefinition writerMimeTypeDefinition = null)
        : this(serviceUri.ToSafeString(), writerMimeTypeDefinition)
    {
    }

    //
    // Summary:
    //     Creates a new connection to a Fuseki Server.
    //
    // Parameters:
    //   serviceUri:
    //     The /data URI of the Fuseki Server.
    //
    //   writerMimeTypeDefinition:
    //     The MIME type of the syntax to use when sending RDF data to the server. Defaults
    //     to RDF/XML.
    public OntotextGraphDBConnector(string serviceUri, MimeTypeDefinition writerMimeTypeDefinition = null)
        : base(serviceUri, writerMimeTypeDefinition)
    {
        bool includeTrailingSlash = serviceUri.EndsWith("/");
        string slash = includeTrailingSlash ? string.Empty : "/";
        _updateUri = serviceUri + slash + "statements";
        _queryUri = serviceUri + slash;
    }

    //
    // Summary:
    //     Creates a new connection to a Fuseki Server.
    //
    // Parameters:
    //   serviceUri:
    //     The /data URI of the Fuseki Server.
    //
    //   proxy:
    //     Proxy Server.
    public OntotextGraphDBConnector(Uri serviceUri, IWebProxy proxy)
        : this(serviceUri.ToSafeString(), proxy)
    {
    }

    //
    // Summary:
    //     Creates a new connection to a Fuseki Server.
    //
    // Parameters:
    //   serviceUri:
    //     The /data URI of the Fuseki Server.
    //
    //   proxy:
    //     Proxy Server.
    public OntotextGraphDBConnector(string serviceUri, IWebProxy proxy)
        : this(serviceUri)
    {
        base.Proxy = proxy;
    }

    //
    // Summary:
    //     Gets the List of Graphs from the store.
    [Obsolete("Replaced by ListGraphNames")]
    public override IEnumerable<Uri> ListGraphs()
    {
        try
        {
            if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet sparqlResultSet)
            {
                List<Uri> list = new List<Uri>();
                foreach (SparqlResult item in sparqlResultSet)
                {
                    if (item.HasValue("g"))
                    {
                        INode node = item["g"];
                        if (node != null && node.NodeType == NodeType.Uri)
                        {
                            list.Add(((IUriNode)node).Uri);
                        }
                    }
                }

                return list;
            }

            throw new RdfStorageException("Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex2)
        {
            throw StorageHelper.HandleError(ex2, "listing Graphs from");
        }
    }

    //
    // Summary:
    //     Gets an enumeration of the names of the graphs in the store.
    //
    // Remarks:
    //     Implementations should implement this method only if they need to provide a custom
    //     way of listing Graphs. If the Store for which you are providing a manager can
    //     efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s
    //     ?p ?o } } query then there should be no need to implement this function.
    public override IEnumerable<string> ListGraphNames()
    {
        try
        {
            if (!(Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet sparqlResultSet))
            {
                throw new RdfStorageException("Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
            }

            List<string> list = new List<string>();
            foreach (SparqlResult item in sparqlResultSet)
            {
                if (!item.HasValue("g"))
                {
                    continue;
                }

                INode node = item["g"];
                if (node != null)
                {
                    switch (node.NodeType)
                    {
                        case NodeType.Uri:
                            list.Add(((IUriNode)node).Uri.AbsoluteUri);
                            break;
                        case NodeType.Blank:
                            list.Add(((IBlankNode)node).InternalID);
                            break;
                    }
                }
            }

            return list;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex2)
        {
            throw StorageHelper.HandleError(ex2, "listing Graphs from");
        }
    }

    //
    // Summary:
    //     Updates a Graph in the Fuseki store.
    //
    // Parameters:
    //   graphUri:
    //     URI of the Graph to update.
    //
    //   additions:
    //     Triples to be added.
    //
    //   removals:
    //     Triples to be removed.
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        try
        {
            string text = ((graphUri != null && !graphUri.Equals(string.Empty)) ? ("GRAPH <" + _formatter.FormatUri(graphUri) + "> {") : string.Empty);
            StringBuilder stringBuilder = new StringBuilder();
            if (additions != null && additions.Any())
            {
                stringBuilder.AppendLine("INSERT DATA {");
                if (!text.Equals(string.Empty))
                {
                    stringBuilder.AppendLine(text);
                }

                foreach (Triple addition in additions)
                {
                    stringBuilder.AppendLine(_formatter.Format(addition));
                }

                if (!text.Equals(string.Empty))
                {
                    stringBuilder.AppendLine("}");
                }

                stringBuilder.AppendLine("}");
            }

            if (removals != null && removals.Any())
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine(";");
                }

                stringBuilder.AppendLine("DELETE DATA {");
                if (!text.Equals(string.Empty))
                {
                    stringBuilder.AppendLine(text);
                }

                foreach (Triple removal in removals)
                {
                    stringBuilder.AppendLine(_formatter.Format(removal));
                }

                if (!text.Equals(string.Empty))
                {
                    stringBuilder.AppendLine("}");
                }

                stringBuilder.AppendLine("}");
            }

            if (stringBuilder.Length <= 0)
            {
                return;
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
            {
                Content = new StringContent(stringBuilder.ToString(), Encoding.UTF8, "application/sparql-update")
            };
            using HttpResponseMessage httpResponseMessage = base.HttpClient.SendAsync(request).Result;
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(httpResponseMessage, "updating a Graph in");
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex2)
        {
            throw StorageHelper.HandleError(ex2, "updating a Graph in");
        }
    }

    //
    // Summary:
    //     Updates a Graph in the Fuseki store.
    //
    // Parameters:
    //   graphUri:
    //     URI of the Graph to update.
    //
    //   additions:
    //     Triples to be added.
    //
    //   removals:
    //     Triples to be removed.
    public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        UpdateGraph(graphUri.ToSafeString(), additions, removals);
    }

    //
    // Summary:
    //     Executes a SPARQL Query on the Fuseki store.
    //
    // Parameters:
    //   sparqlQuery:
    //     SPARQL Query.
    public object Query(string sparqlQuery)
    {
        Graph graph = new Graph();
        SparqlResultSet sparqlResultSet = new SparqlResultSet();
        Query(new GraphHandler(graph), new ResultSetHandler(sparqlResultSet), sparqlQuery);
        if (sparqlResultSet.ResultsType != SparqlResultsType.Unknown)
        {
            return sparqlResultSet;
        }

        return graph;
    }

    //
    // Summary:
    //     Executes a SPARQL Query on the Fuseki store processing the results using an appropriate
    //     handler from those provided.
    //
    // Parameters:
    //   rdfHandler:
    //     RDF Handler.
    //
    //   resultsHandler:
    //     Results Handler.
    //
    //   sparqlQuery:
    //     SPARQL Query.
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        try
        {
            string queryUri = _queryUri.Trim('/');
            HttpRequestMessage httpRequestMessage;

            httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, queryUri);
            httpRequestMessage.Headers.Add("Accept", MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);
            httpRequestMessage.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[1]
            {
                    new KeyValuePair<string, string>("query", sparqlQuery)
            });

            using HttpResponseMessage httpResponseMessage = base.HttpClient.SendAsync(httpRequestMessage).Result;
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                Stream s = httpResponseMessage.Content.ReadAsStream();
                StreamReader sr = new StreamReader(s);
                var error = sr.ReadToEnd();
                throw StorageHelper.HandleHttpQueryError(httpResponseMessage);
            }

            StreamReader input = new StreamReader(httpResponseMessage.Content.ReadAsStreamAsync().Result);
            string mediaType = httpResponseMessage.Content.Headers.ContentType.MediaType;
            try
            {
                IRdfReader parser = MimeTypesHelper.GetParser(mediaType);
                parser.Load(rdfHandler, input);
            }
            catch (RdfParserSelectionException)
            {
                ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(mediaType, allowPlainTextResults: true);
                sparqlParser.Load(resultsHandler, input);
            }
        }
        catch (RdfQueryException)
        {
            throw;
        }
        catch (Exception ex3)
        {
            throw StorageHelper.HandleError(ex3, "querying");
        }
    }

    //
    // Summary:
    //     Executes SPARQL Updates against the Fuseki store.
    //
    // Parameters:
    //   sparqlUpdate:
    //     SPARQL Update.
    public void Update(string sparqlUpdate)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
            {
                Content = new StringContent(sparqlUpdate, Encoding.UTF8, "application/sparql-update")
            };
            using HttpResponseMessage httpResponseMessage = base.HttpClient.SendAsync(request).Result;
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(httpResponseMessage, "updating");
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex2)
        {
            throw StorageHelper.HandleError(ex2, "updating");
        }
    }

    //
    // Summary:
    //     Makes a SPARQL Query against the underlying store.
    //
    // Parameters:
    //   sparqlQuery:
    //     SPARQL Query.
    //
    //   callback:
    //     Callback.
    //
    //   state:
    //     State to pass to the callback.
    //
    // Returns:
    //     SparqlResultSet or a Graph depending on the Sparql Query.
    public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
    {
        Graph g = new Graph();
        SparqlResultSet results = new SparqlResultSet();
        Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, delegate (object sender, AsyncStorageCallbackArgs args, object st)
        {
            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, results, args.Error), state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, g, args.Error), state);
            }
        }, state);
    }

    //
    // Summary:
    //     Executes a SPARQL Query on the Fuseki store processing the results using an appropriate
    //     handler from those provided.
    //
    // Parameters:
    //   rdfHandler:
    //     RDF Handler.
    //
    //   resultsHandler:
    //     Results Handler.
    //
    //   sparqlQuery:
    //     SPARQL Query.
    //
    //   callback:
    //     Callback.
    //
    //   state:
    //     State to pass to the callback.
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = CreateQueryRequestMessage(sparqlQuery);
            base.HttpClient.SendAsync(request).ContinueWith(delegate (Task<HttpResponseMessage> requestTask)
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, requestTask.IsCanceled ? new RdfStorageException("The operation was cancelled") : StorageHelper.HandleError(requestTask.Exception, "querying")), state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpError(response, "querying")), state);
                    }
                    else
                    {
                        response.Content.ReadAsStreamAsync().ContinueWith(delegate (Task<Stream> readTask)
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, readTask.IsCanceled ? new RdfStorageException("The operation was cancelled") : StorageHelper.HandleError(readTask.Exception, "querying")), state);
                                return;
                            }

                            try
                            {
                                StreamReader input = new StreamReader(readTask.Result);
                                string mediaType = response.Content.Headers.ContentType.MediaType;
                                try
                                {
                                    ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(mediaType, allowPlainTextResults: true);
                                    sparqlParser.Load(resultsHandler, input);
                                }
                                catch (RdfParserSelectionException)
                                {
                                    IRdfReader parser = MimeTypesHelper.GetParser(mediaType);
                                    parser.Load(rdfHandler, input);
                                }

                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);
                            }
                            catch (Exception ex3)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleError(ex3, "querying")), state);
                            }
                        });
                    }
                }
            }).ConfigureAwait(continueOnCapturedContext: true);
        }
        catch (Exception ex)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleError(ex, "querying")), state);
        }
    }

    private HttpRequestMessage CreateQueryRequestMessage(string sparqlQuery)
    {
        string queryUri = _queryUri;
        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, queryUri);
        httpRequestMessage.Headers.Add("Accept", MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);
        httpRequestMessage.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[1]
        {
            new KeyValuePair<string, string>("query", sparqlQuery)
        });
        return httpRequestMessage;
    }

    public async Task<object> QueryAsync(string sparqlQuery, CancellationToken cancellationToken)
    {
        Graph g = new Graph();
        SparqlResultSet results = new SparqlResultSet();
        await QueryAsync(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, cancellationToken);
        return (results.ResultsType == SparqlResultsType.Unknown) ? ((object)g) : ((object)results);
    }

    public async Task QueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateQueryRequestMessage(sparqlQuery);
        HttpResponseMessage response = await base.HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpQueryError(response);
        }

        try
        {
            StreamReader input = new StreamReader(await response.Content.ReadAsStreamAsync());
            string mediaType = response.Content.Headers.ContentType.MediaType;
            try
            {
                ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(mediaType, allowPlainTextResults: true);
                sparqlParser.Load(resultsHandler, input);
            }
            catch (RdfParserSelectionException)
            {
                IRdfReader parser = MimeTypesHelper.GetParser(mediaType);
                parser.Load(rdfHandler, input);
            }
        }
        catch (Exception ex2)
        {
            throw StorageHelper.HandleError(ex2, "querying");
        }
    }

    //
    // Summary:
    //     Executes SPARQL Updates against the Fuseki store.
    //
    // Parameters:
    //   sparqlUpdate:
    //     SPARQL Update.
    //
    //   callback:
    //     Callback.
    //
    //   state:
    //     State to pass to the callback.
    public void Update(string sparqlUpdate, AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
            {
                Content = new StringContent(sparqlUpdate, Encoding.UTF8, "application/sparql-update")
            };
            base.HttpClient.SendAsync(request).ContinueWith(delegate (Task<HttpResponseMessage> requestTask)
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, requestTask.IsCanceled ? new RdfStorageException("The operation was cancelled") : StorageHelper.HandleError(requestTask.Exception, "updating")), state);
                }
                else
                {
                    HttpResponseMessage result = requestTask.Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(result, "updating")), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                    }
                }
            }).ConfigureAwait(continueOnCapturedContext: true);
        }
        catch (Exception ex)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
        }
    }

    public async Task UpdateAsync(string sparqlUpdates, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
        {
            Content = new StringContent(sparqlUpdates, Encoding.UTF8, "application/sparql-update")
        };
        HttpResponseMessage httpResponseMessage = await base.HttpClient.SendAsync(request, cancellationToken);
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(httpResponseMessage, "updating");
        }
    }

    //
    // Summary:
    //     Lists the graph sin the Store asynchronously.
    //
    // Parameters:
    //   callback:
    //     Callback.
    //
    //   state:
    //     State to pass to the callback.
    [Obsolete("Replaced with ListGraphsAsync(CancellationToken)")]
    public override void ListGraphs(AsyncStorageCallback callback, object state)
    {
        ListUrisHandler handler = new ListUrisHandler("g");
        ((IAsyncQueryableStorage)this).Query((IRdfHandler)null, (ISparqlResultsHandler)handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", (AsyncStorageCallback)delegate (object sender, AsyncStorageCallbackArgs args, object st)
        {
            if (args.WasSuccessful)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, handler.Uris), state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, args.Error), state);
            }
        }, state);
    }

    public override async Task<IEnumerable<string>> ListGraphsAsync(CancellationToken cancellationToken)
    {
        ListUrisHandler handler = new ListUrisHandler("g");
        await QueryAsync(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", cancellationToken);
        return handler.Uris.Select((Uri u) => u?.AbsoluteUri);
    }

    //
    // Summary:
    //     Updates a Graph on the Fuseki Server.
    //
    // Parameters:
    //   graphUri:
    //     URI of the Graph to update.
    //
    //   additions:
    //     Triples to be added.
    //
    //   removals:
    //     Triples to be removed.
    //
    //   callback:
    //     Callback.
    //
    //   state:
    //     State to pass to the callback.
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            MakeSparqlUpdate(graphUri, additions, removals, stringBuilder);
            if (stringBuilder.Length > 0)
            {
                Update(stringBuilder.ToString(), delegate (object sender, AsyncStorageCallbackArgs args, object st)
                {
                    if (args.WasSuccessful)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                    }
                }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
        }
        catch (Exception ex)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "updating a Graph asynchronously")), state);
        }
    }

    private void MakeSparqlUpdate(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, StringBuilder update)
    {
        string text = ((graphUri != null && !graphUri.Equals(string.Empty)) ? ("GRAPH <" + _formatter.FormatUri(graphUri) + "> {") : string.Empty);
        if (additions != null && additions.Any())
        {
            update.AppendLine("INSERT DATA {");
            if (!text.Equals(string.Empty))
            {
                update.AppendLine(text);
            }

            foreach (Triple addition in additions)
            {
                update.AppendLine(_formatter.Format(addition));
            }

            if (!text.Equals(string.Empty))
            {
                update.AppendLine("}");
            }

            update.AppendLine("}");
        }

        if (removals == null || !removals.Any())
        {
            return;
        }

        if (update.Length > 0)
        {
            update.AppendLine(";");
        }

        update.AppendLine("DELETE DATA {");
        if (!text.Equals(string.Empty))
        {
            update.AppendLine(text);
        }

        foreach (Triple removal in removals)
        {
            update.AppendLine(_formatter.Format(removal));
        }

        if (!text.Equals(string.Empty))
        {
            update.AppendLine("}");
        }

        update.AppendLine("}");
    }

    public override async Task UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, CancellationToken cancellationToken)
    {
        StringBuilder stringBuilder = new StringBuilder();
        MakeSparqlUpdate(graphUri, additions, removals, stringBuilder);
        if (stringBuilder.Length > 0)
        {
            await UpdateAsync(stringBuilder.ToString(), cancellationToken);
        }
    }

    //
    // Summary:
    //     Gets a String which gives details of the Connection.
    public override string ToString()
    {
        return "[Ontotext] " + _serviceUri;
    }

    //
    // Summary:
    //     Serializes the connection's configuration.
    //
    // Parameters:
    //   context:
    //     Configuration Serialization Context.
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode nextSubject = context.NextSubject;
        INode pred = context.Graph.CreateUriNode(context.UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
        INode pred2 = context.Graph.CreateUriNode(context.UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#label"));
        INode pred3 = context.Graph.CreateUriNode(context.UriFactory.Create("http://www.dotnetrdf.org/configuration#type"));
        INode obj = context.Graph.CreateUriNode(context.UriFactory.Create("http://www.dotnetrdf.org/configuration#StorageProvider"));
        INode pred4 = context.Graph.CreateUriNode(context.UriFactory.Create("http://www.dotnetrdf.org/configuration#server"));
        context.Graph.Assert(new Triple(nextSubject, pred, obj));
        context.Graph.Assert(new Triple(nextSubject, pred2, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(nextSubject, pred3, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(nextSubject, pred4, context.Graph.CreateLiteralNode(_serviceUri)));
    }
}