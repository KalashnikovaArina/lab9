﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LAB9
{
    public partial class Form1 : Form
    {

        internal class Transform
        {
            public Vector3 Position;

            public Vector3 Direction;
            public Vector3 Top;
            public Vector3 Right;

            const double cameraRotationSpeed = 1;
            double yaw = 0.0, pitch = 0.0;
            double zScreenNear = 1;
            double zScreenFar = 100;
            double fov = 45;
            public PointF worldCenter;
            int screenWidth, screenHeight;
            double[,] parallelProjectionMatrix, perspectiveProjectionMatrix;

            public Transform(int screenWidth, int screenHeight)
            {
                Position = new Vector3(-200, 0, 0);
                Direction = new Vector3(1, 0, 0);
                Top = new Vector3(0, 0, 1);
                Right = (Direction * Top).Normalize();
                this.screenHeight = screenHeight;
                this.screenWidth = screenWidth;
                worldCenter = new PointF(screenWidth / 2, screenHeight / 2);
                UpdateProjMatrix();
            }

            public void UpdateProjMatrix()
            {
                parallelProjectionMatrix = new double[,] {
                    { 1.0 / screenWidth, 0,                       0,                                 0},
                    { 0,                      1.0 / screenHeight, 0,                                 0},
                    { 0,                      0,                       -2.0 / (zScreenFar - zScreenNear), -(zScreenFar + zScreenNear) / (zScreenFar - zScreenNear)},
                    { 0,                      0,                        0,                                 1}
                };

                perspectiveProjectionMatrix = new double[,] {
                    { screenHeight / (Math.Tan(AffineTransformations.DegreesToRadians(fov / 2)) * screenWidth), 0, 0, 0},
                    { 0, 1.0 / Math.Tan(AffineTransformations.DegreesToRadians(fov / 2)), 0, 0},
                    { 0, 0, -(zScreenFar + zScreenNear) / (zScreenFar - zScreenNear), -2 * (zScreenFar * zScreenNear) / (zScreenFar - zScreenNear)},
                    { 0, 0, -1, 0}
                };



            }
            public Vertex toCameraView(Vertex v)
            {
                return new Vertex(Right.x * (v.x - Position.x) + Right.y * (v.y - Position.y) + Right.z * (v.z - Position.z),
                                  Top.x * (v.x - Position.x) + Top.y * (v.y - Position.y) + Top.z * (v.z - Position.z),
                                  Direction.x * (v.x - Position.x) + Direction.y * (v.y - Position.y) + Direction.z * (v.z - Position.z)
                                  );
            }


            internal Vertex to2D(Vertex v, ProjectionMode proj)
            {
                var viewCoord = this.toCameraView(v);

                switch (proj)
                {
                    case ProjectionMode.Other:
                        // if (viewCoord.z > 0)
                        {
                            return new Vertex(worldCenter.X + (float)viewCoord.x, worldCenter.Y + viewCoord.y, viewCoord.z);
                        }
                    // else
                    //    return null;

                    case ProjectionMode.Perspective:
                        //if (viewCoord.z < 0)
                        //{
                        //    return null;
                        //}

                        var res = AffineTransformations.Multiply(new double[,] { { viewCoord.x, viewCoord.y, viewCoord.z, 1 } }, perspectiveProjectionMatrix);
                        //if (res[0, 3] == 0)
                        //{
                        //    return null;

                        //}

                        var elem = 1.0 / res[0, 3];
                        for (int i = 0; i < res.GetLength(0); i++)
                        {
                            for (int j = 0; j < res.GetLength(1); j++)
                            {
                                res[i, j] *= elem;
                            }
                        }

                        res[0, 0] = Transform.Clamp(res[0, 0], -1, 1);
                        res[0, 1] = Transform.Clamp(res[0, 1], -1, 1);

                        //if (res[0, 2] < 0)
                        //{
                        //    return null;
                        //}
                        return new Vertex(worldCenter.X + res[0, 0] * worldCenter.X, worldCenter.Y + res[0, 1] * worldCenter.Y, (float)v.z);
                    default:
                        return null;

                }



            }

            //x = (x < a) ? a : ((x > b) ? b : x);
            public static double Clamp(double val, double min, double max)
            {
                if (val.CompareTo(min) < 0) return min;
                else if (val.CompareTo(max) > 0) return max;
                else return val;
            }
        }

        public void RedrawCamryField()
        {
            //g2.Clear(Color.White);
            Vertex line_start, line_end;
            foreach (var obj in objects_list.Items)
            {
                var cur_poly = obj as Polyhedron;
                for (int i = 0; i < cur_poly.vertices.Count; i++)
                {

                    line_start = trans.to2D(cur_poly.vertices[i], ProjectionMode.Other);
                    //line_start = new Vertex(cur_m[0, 0], cur_m[0, 1], 0);

                    //пробегает по всем граничным точкам и рисует линию
                    for (int j = 0; j < cur_poly.edges[i].Count; j++)
                    {
                        var ind = cur_poly.edges[i][j];

                        line_end = trans.to2D(cur_poly.vertices[ind], ProjectionMode.Other);
                        //if (null != line_start && null != line_end)
                        //if (!(line_start is null || line_end is null))
                        //    g2.DrawLine(pen, (float)line_start.x, (float)line_start.y, (float)line_end.x, (float)line_end.y);
                    }
                }
            }
        }



    }

    
}