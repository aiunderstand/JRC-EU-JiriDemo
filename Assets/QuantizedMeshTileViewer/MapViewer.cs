﻿using BruTile;
using Terrain.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terrain.Tiles;
using UnityEngine;
using UnityEngine.Networking;
using System.Diagnostics;
using System;

namespace Terrain
{
    public class MapViewer : MonoBehaviour
    {
        [SerializeField]
        private readonly Extent extent = new Extent(4.77, 52.28, 4.97, 52.41); //part of Amsterdam, Netherlands in Latitude/Longitude (GPS) coordinates as boundingbox.
       
        [SerializeField]
        private int zoomLevel = 14;
        [SerializeField]
        private GameObject placeholderTile;
        [SerializeField]
        private string terrainUrl = @"https://saturnus.geodan.nl/tomt/data/tiles/{z}/{x}/{y}.terrain?v=1.0.0";
        [SerializeField]
        private string textureUrl = @"https://saturnus.geodan.nl/mapproxy/bgt/service?crs=EPSG%3A3857&service=WMS&version=1.1.1&request=GetMap&styles=&format=image%2Fjpeg&layers=bgt&bbox={xMin}%2C{yMin}%2C{xMax}%2C{yMax}&width=256&height=256&srs=EPSG%3A4326";
        [SerializeField]
        private string buildingsUrl = @"https://saturnus.geodan.nl/tomt/data/buildingtiles_adam/tiles/{id}.b3dm";

        readonly Dictionary<Vector3, GameObject> tileDb = new Dictionary<Vector3, GameObject>();

        const int maxParallelRequests = 4;
        Queue<downloadRequest> downloadQueue = new Queue<downloadRequest>();
        Dictionary<string, downloadRequest> pendingQueue = new Dictionary<string, downloadRequest>(maxParallelRequests);

        Viewport viewport;
        float yOffset = 0;
        float xOffset = 0;

        //debuggin'
        private Stopwatch sw = new Stopwatch();
        int processedTileDebugCounter = 0;
        
        public enum TileService
        {
            WMS,
            QM
        }

        public struct downloadRequest
        {

            public string Url;
            public TileService Service;
            public Vector3 TileId;
           
            public downloadRequest(string url, TileService service, Vector3 tileId)
            {
                Url = url;
                Service = service;
                TileId = tileId;
            }
        }

        public void CreateTiles()
        {
            ClearTiles();
            sw.Start();
            
            var schema = new TmsGlobalGeodeticTileSchema();
            var tileRange = TileTransform.WorldToTile(extent, zoomLevel.ToString(), schema);

            var tilesOrigin = schema.GetTileInfos(extent, 13.ToString()).ToList();   //refactor when done experimenting scale
            var tiles = schema.GetTileInfos(extent, zoomLevel.ToString()).ToList();
            yOffset = (float) Math.Abs(tiles[0].Extent.MinY - tilesOrigin[0].Extent.MinY);
            xOffset = (float) Math.Abs(tiles[0].Extent.MinX - tilesOrigin[0].Extent.MinX);
            UnityEngine.Debug.Log(xOffset);

            var folder = new GameObject("map");
             //immediately draw placeholder tile and fire request for texture and height. Depending on which one returns first, update place holder.
            foreach (var t in tiles)
            {
                //draw placeholder tile
                GameObject tile = DrawPlaceHolder(tileRange, t);
                tile.transform.parent = folder.transform;

                tileDb.Add(new Vector3(t.Index.Col, t.Index.Row, int.Parse(t.Index.Level)), tile);

                //get tile texture data
                Extent subtileExtent = TileTransform.TileToWorld(new TileRange(t.Index.Col, t.Index.Row), t.Index.Level.ToString(), schema);
                var wmsUrl = textureUrl.Replace("{xMin}", subtileExtent.MinX.ToString()).Replace("{yMin}", subtileExtent.MinY.ToString()).Replace("{xMax}", subtileExtent.MaxX.ToString()).Replace("{yMax}", subtileExtent.MaxY.ToString()).Replace(",", ".");
                downloadQueue.Enqueue(new downloadRequest(wmsUrl, TileService.WMS, new Vector3(t.Index.Col, t.Index.Row, int.Parse(t.Index.Level))));

                //get tile height data (
                var qmUrl = terrainUrl.Replace("{x}", t.Index.Col.ToString()).Replace("{y}", t.Index.Row.ToString()).Replace("{z}", int.Parse(t.Index.Level).ToString());
                downloadQueue.Enqueue(new downloadRequest(qmUrl, TileService.QM, new Vector3(t.Index.Col, t.Index.Row, int.Parse(t.Index.Level))));

                
            }
        }

        private GameObject DrawPlaceHolder(TileRange tileRange, TileInfo t)
        {
            var tile = Instantiate(placeholderTile);
            tile.name = $"tile/{t.Index.ToIndexString()}";
            tile.transform.position = GetTilePosition(t.Index, tileRange);

            tile.transform.localScale = new Vector3(360 * ComputeScaleFactor(int.Parse(t.Index.Level)),360 * ComputeScaleFactor(int.Parse(t.Index.Level)) , 1); //should be refactored when experimenting with offset and scale is done
            return tile;
        }

        private void ClearTiles()
        {
            Destroy(GameObject.Find("map"));
            tileDb.Clear();
            downloadQueue.Clear();
            pendingQueue.Clear();
            sw.Reset();
            processedTileDebugCounter = 0;
        }

        private Vector3 GetTilePosition(TileIndex index, TileRange tileRange)
        {
            float tileOffsetRow = -7000 + yOffset * 360 * ComputeScaleFactor(int.Parse(index.Level)) +(0.5f * 360 * ComputeScaleFactor(int.Parse(index.Level))); //makes sure that tile left corner is in CENTER
            float tileOffsetCol = -9000 + xOffset * 360 * ComputeScaleFactor(int.Parse(index.Level)) +(0.5f * 360 * ComputeScaleFactor(int.Parse(index.Level))); //makes sure that tile left corner is in CENTER

            return new Vector3((index.Col - tileRange.FirstCol) * (-360 * ComputeScaleFactor(int.Parse(index.Level))) - tileOffsetCol, 0, (index.Row - tileRange.FirstRow) * (360 * ComputeScaleFactor(int.Parse(index.Level))) - tileOffsetRow); //should be refactored when experimenting with offset and scale is done
        }

        private IEnumerator requestQMTile(string url, Vector3 tileId)
        {
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            TerrainTile terrainTile;
            UnityWebRequest www = new UnityWebRequest(url);

            www.downloadHandler = handler;
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                //get data
                MemoryStream stream = new MemoryStream(www.downloadHandler.data);

                //parse into tile data structure
                terrainTile = TerrainTileParser.Parse(stream);

                //update tile with height data
                tileDb[tileId].GetComponent<MeshFilter>().sharedMesh = terrainTile.GetMesh(-44); //height offset is manually done to nicely align height data with place holder at 0
                tileDb[tileId].transform.rotation = Quaternion.Euler(new Vector3(180, 0, 0));
                tileDb[tileId].transform.localScale = new Vector3(ComputeScaleFactor((int) tileId.z), 1, ComputeScaleFactor((int)tileId.z) *2);
            }
            else
            {
                UnityEngine.Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading height data");
            }

            pendingQueue.Remove(url);
            processedTileDebugCounter++;

            if (pendingQueue.Count == 0)
                UnityEngine.Debug.Log("finished: with max queue size " + maxParallelRequests + ". Time: " + sw.Elapsed.TotalMilliseconds + " miliseconds. Total requests: " + processedTileDebugCounter);
        }

        private float ComputeScaleFactor(int z)
        {
            //this should be refactored to a Math.pow(2, level) formula, after experimenting with computing real space : unity space.
            switch (z)
            {
                case 13:
                    return 1f;
                case 14:
                    return 0.5f;
                case 15:
                    return 0.25f;
                case 16:
                    return 0.125f;
                case 17:
                    return 0.0625f;
                default:
                    return 1f;
            }
        }

        private IEnumerator requestWMSTile(string url, Vector3 tileId)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                //update tile with height data
                tileDb[tileId].GetComponent<MeshRenderer>().material.mainTexture = myTexture;
            }
            else
            {
                UnityEngine.Debug.LogError("Tile: [" + tileId.x + " " + tileId.y + "] Error loading texture data");
            }

            pendingQueue.Remove(url);
            processedTileDebugCounter++;

            if (pendingQueue.Count == 0)
                UnityEngine.Debug.Log("finished: with max queue size " + maxParallelRequests + ". Time: " + sw.Elapsed.TotalMilliseconds + " miliseconds. Total requests: " + processedTileDebugCounter);
        }

        public void Update()
        {

            if (pendingQueue.Count < maxParallelRequests && downloadQueue.Count > 0)
            {
                var request = downloadQueue.Dequeue();
                pendingQueue.Add(request.Url, request);

                //fire request
                switch (request.Service)
                {
                    case TileService.QM:
                        StartCoroutine(requestQMTile(request.Url, request.TileId));
                        break;
                    case TileService.WMS:
                        StartCoroutine(requestWMSTile(request.Url, request.TileId));
                        break;
                }
            }
        }

        public void ZoomIn()
        {
            zoomLevel++;
            CreateTiles();
        }

        public void ZoomOut()
        {
            zoomLevel--;
            CreateTiles();
        }
    }


}