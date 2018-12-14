using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShapeFilesParser;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using ShapeFilesParser.Business;
using ShapeFilesParser.Business.Models;
using System;
using System.IO;

public static class ShapeImporter
{
    public static List<Record<PointZ>> ReadShapes(string path)
    {
        var x = new ShapeFilesParser.Business.ShapeManager();
        var type = x.GetGeometryType(path + ".shp" );
        switch (type)
        {
            case GeometryType.PointZ:
                {
                    return x.GetShapes(path, new ShapeFilesParser.Business.Parsers.PointZParser());
                }
        }

        return null;
    }
}