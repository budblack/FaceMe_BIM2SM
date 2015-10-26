using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SuperMap.Data;
using SuperMap.Realspace;
using SuperMap.UI;

namespace FaceMe
{
    class Program
    {
        static void Main(string[] args)
        {
            string wsPath = @"D:\模型找正面\ws.smwu";

            Workspace ws = new Workspace();
            WorkspaceConnectionInfo wsCon = new WorkspaceConnectionInfo()
            {
                Server = wsPath,
                Type = WorkspaceType.SMWU
            };
            
            ws.Open(wsCon);

            foreach (Datasource datasource in ws.Datasources)
            {
                foreach (Dataset dataset in datasource.Datasets)
                {
                    switch (dataset.Type)
                    {
                        case DatasetType.CAD:
                        //case DatasetType.Model:
                            Console.WriteLine(dataset.Name);
                            Phineas p = new Phineas()
                            {
                                dv = dataset as DatasetVector
                            };
                            p.run();
                            break;
                    }
                }
            }

            Console.Read();
        }
    }

    class Phineas
    {
        public DatasetVector dv;
        Random R = new Random();

        public void run()
        {
            Recordset rc = dv.GetRecordset(false, CursorType.Dynamic);

            Dictionary<int, Feature> feas = rc.GetAllFeatures();

            foreach (KeyValuePair<int, Feature> item in feas)
            {
                GeoModel gm = item.Value.GetGeometry() as GeoModel;
                //Console.WriteLine("==" + gm.Position + "==");

                GeoModel model = new GeoModel();
                model.Position = gm.Position;
                foreach (Mesh m in gm.Meshes)
                {
                    if (m.Material.TextureFile.Length > 1)
                    {
                        //Console.WriteLine(m.Material.TextureFile.ToString());
                        Point3Ds p3ds = new Point3Ds();

                        for (int i = 0; i < m.Vertices.Length; i += 3)
                        {
                            bool repition = false;
                            foreach (Point3D p in p3ds)
                            {
                                if (p.X == m.Vertices[i] && p.Y == m.Vertices[i + 1] && p.Z == m.Vertices[i + 2])
                                {
                                    repition = true;
                                }
                            }
                            if (!repition)
                            {
                                p3ds.Add(new Point3D(m.Vertices[i], m.Vertices[i + 1], m.Vertices[i + 2]));

                            }
                        }

                        //foreach (Point3D p3d in p3ds)
                        //{
                        //    Console.WriteLine(string.Format(" {0},{1},{2}", p3d.X, p3d.Y, p3d.Z));
                        //}

                        #region 写属性表
                        Dictionary<string, double> fields = new Dictionary<string, double>();
                        fields.Add("FaceMeshCenterX", model.Position.X);
                        fields.Add("FaceMeshCenterY", model.Position.Y);
                        fields.Add("FaceMeshCenterZ", model.Position.Z);
                        fields.Add("FaceMeshLx", p3ds.leftbottom().X);
                        fields.Add("FaceMeshLy", p3ds.leftbottom().Y);
                        fields.Add("FaceMeshLz", p3ds.leftbottom().Z);
                        fields.Add("FaceMeshUx", p3ds.rightup().X);
                        fields.Add("FaceMeshUy", p3ds.rightup().Y);
                        fields.Add("FaceMeshUz", p3ds.rightup().Z);


                        foreach (KeyValuePair<string, double> field in fields)
                        {
                            if (dv.FieldInfos.IndexOf(field.Key) < 0)
                            {
                                FieldInfo fieldInf = new FieldInfo(field.Key, FieldType.Double);
                                dv.FieldInfos.Add(fieldInf);
                            }

                            string fieldName = field.Key;
                            double fieldValue = field.Value;
                            try
                            {
                                rc.SeekID(item.Value.GetID());
                                rc.Edit();
                                rc.SetFieldValue(fieldName, fieldValue);
                                rc.Update();
                            }
                            catch
                            {
                                Console.WriteLine("error!");
                            }
                            //Console.WriteLine(string.Format("{0},{1},{2}", item.GetID(), fieldName, fieldValue));
                        }
                        #endregion
                    }
                }
                //Console.WriteLine("");
            }
            Console.WriteLine(dv.Name+" done.");
        }
    }

    public static class Ext
    {
        public static Point3D leftbottom(this Point3Ds p3ds)
        {
            Point3D p = p3ds[0];

            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X > p3ds[i].X)
                {
                    p = p3ds[i];
                }
            }
            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X == p3ds[i].X && p.Y > p3ds[i].Y)
                {
                    p = p3ds[i];
                }
            }
            p.Z = Math.Min(Math.Min(p3ds[0].Z, p3ds[1].Z), p3ds[2].Z);

            return p;
        }

        public static Point3D leftup(this Point3Ds p3ds)
        {
            Point3D p = p3ds[0];

            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X > p3ds[i].X)
                {
                    p = p3ds[i];
                }
            }
            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X == p3ds[i].X && p.Y > p3ds[i].Y)
                {
                    p = p3ds[i];
                }
            }
            p.Z = Math.Max(Math.Max(p3ds[0].Z, p3ds[1].Z), p3ds[2].Z);

            return p;
        }

        public static Point3D rightup(this Point3Ds p3ds)
        {
            Point3D p = p3ds[0];

            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X < p3ds[i].X)
                {
                    p = p3ds[i];
                }
            }
            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X == p3ds[i].X && p.Y < p3ds[i].Y)
                {
                    p = p3ds[i];
                }
            }
            p.Z = Math.Max(Math.Max(p3ds[0].Z, p3ds[1].Z), p3ds[2].Z);

            return p;
        }

        public static Point3D rightbottom(this Point3Ds p3ds)
        {
            Point3D p = p3ds[0];

            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X < p3ds[i].X)
                {
                    p = p3ds[i];
                }
            }
            for (int i = 1; i < p3ds.Count; i++)
            {
                if (p.X == p3ds[i].X && p.Y < p3ds[i].Y)
                {
                    p = p3ds[i];
                }
            }
            p.Z = Math.Min(Math.Min(p3ds[0].Z, p3ds[1].Z), p3ds[2].Z);

            return p;
        }
    }
}
