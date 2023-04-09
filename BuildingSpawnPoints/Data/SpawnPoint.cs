using HarmonyLib;
using ModsCommon;
using ModsCommon.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;
using UnityEngine;

namespace BuildingSpawnPoints
{
    public class BuildingSpawnPoint : IToXml
    {
        public static string XmlName => "P";

        public BuildingData Data { get; set; }
        public Action OnChanged { get; set; }

        public PropertyULongEnumValue<VehicleCategory> Categories { get; }
        public IEnumerable<VehicleCategory> SelectedCategories => Categories.Value.GetEnumValues().IsItem();
        public IEnumerable<VehicleCategory> MissedCategories
        {
            get
            {
                var missed = Data.PossibleVehicles & ~Categories.Value;
                return missed.GetEnumValues().IsItem();
            }
        }

        public PropertyEnumValue<PointType> Type { get; }
        public PropertyStructValue<Vector4> Position { get; set; }
        public PropertyBoolValue FollowGround { get; set; }

        private Dictionary<VehicleService, PathUnit.Position> Services { get; } = new Dictionary<VehicleService, PathUnit.Position>();
        public bool IsCorrect
        {
            get
            {
                foreach (var service in EnumExtension.GetEnumValues<VehicleService>())
                {
                    if ((Categories.Value & service.GetCategory()) != 0 && !IsServiceCorrect(service))
                        return false;
                }

                return true;
            }
        }

        public string XmlSection => XmlName;

        private delegate WaterSimulation.Cell[] WaterSimulationCellDelegate(WaterSimulation waterSimulation);
        private static WaterSimulationCellDelegate WaterCellsDelegate { get; set; }
        private static WaterSimulation.Cell[] WaterCells => WaterCellsDelegate.Invoke(TerrainManager.instance.WaterSimulation);
        private static Mesh Mesh { get; set; }
        private static Material Material { get; set; }
        private static MaterialPropertyBlock MaterialProperty { get; set; }
        private static int ID_Color { get; set; }
        private static int Layer { get; set; } = 9;
        static BuildingSpawnPoint()
        {
            var definition = new DynamicMethod("GetWaterBuffers", typeof(WaterSimulation.Cell[]), new Type[1] { typeof(WaterSimulation) }, true);
            var generator = definition.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(WaterSimulation), "m_waterBuffers"));
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, AccessTools.Field(typeof(WaterSimulation), "m_waterFrameIndex"));
            generator.Emit(OpCodes.Ldc_I4_6);
            generator.Emit(OpCodes.Shr_Un);
            generator.Emit(OpCodes.Not);
            generator.Emit(OpCodes.Ldc_I4_1);
            generator.Emit(OpCodes.And);
            generator.Emit(OpCodes.Conv_U);
            generator.Emit(OpCodes.Ldelem_Ref);
            generator.Emit(OpCodes.Ret);
            WaterCellsDelegate = (WaterSimulationCellDelegate)definition.CreateDelegate(typeof(WaterSimulationCellDelegate));
        }
        public static void CreateMarker()
        {
            try
            {
                SingletonMod<Mod>.Logger.Debug("Start creating marker");

                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                var sphereMesh = sphere.GetComponent<MeshFilter>().sharedMesh;
                var sphereVertices = sphereMesh.vertices;
                var sphereTriangles = sphereMesh.triangles;
                var sphereNormals = sphereMesh.normals;
                GameObject.Destroy(sphere);

                for (var i = 0; i < sphereVertices.Length; i += 1)
                {
                    sphereVertices[i] = sphereVertices[i] * 2f;
                    var distance = sphereVertices[i].XZ().magnitude;
                    if (distance <= 1.75f)
                        distance = 0.25f;
                    else
                    {
                        distance -= 1.75f;
                        distance = Mathf.Sqrt(0.0625f - distance * distance);
                    }
                    sphereVertices[i].y = Mathf.Clamp(sphereVertices[i].y, -distance, distance);
                }

                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                var capsuleMesh = capsule.GetComponent<MeshFilter>().sharedMesh;
                var capsuleVertices = capsuleMesh.vertices;
                var capsuleTriangles = capsuleMesh.triangles;
                var capsuleNormals = capsuleMesh.normals;
                GameObject.Destroy(capsule);

                for (var i = 0; i < capsuleVertices.Length; i += 1)
                {
                    capsuleVertices[i].y += capsuleVertices[i].y > 0 ? 1f : -1f;
                    capsuleVertices[i] = new Vector3(capsuleVertices[i].y * 0.4f + 1.4f, -capsuleVertices[i].x * 0.4f, capsuleVertices[i].z * 0.4f);
                    capsuleNormals[i] = new Vector3(capsuleNormals[i].y, -capsuleNormals[i].x, capsuleNormals[i].z);
                }

                var vertices = new Vector3[sphereVertices.Length + capsuleVertices.Length];
                Array.Copy(sphereVertices, 0, vertices, 0, sphereVertices.Length);
                Array.Copy(capsuleVertices, 0, vertices, sphereVertices.Length, capsuleVertices.Length);

                var triangles = new int[sphereTriangles.Length + capsuleTriangles.Length];
                Array.Copy(sphereTriangles, 0, triangles, 0, sphereTriangles.Length);
                for (var i = 0; i < capsuleTriangles.Length; i += 1)
                    triangles[i + sphereTriangles.Length] = capsuleTriangles[i] + sphereVertices.Length;

                var normals = new Vector3[sphereNormals.Length + capsuleNormals.Length];
                Array.Copy(sphereNormals, 0, normals, 0, sphereNormals.Length);
                Array.Copy(capsuleNormals, 0, normals, sphereNormals.Length, capsuleNormals.Length);


                Mesh = new Mesh()
                {
                    vertices = vertices,
                    triangles = triangles,
                    normals = normals,
                };
                Mesh.RecalculateNormals();
                Mesh.RecalculateTangents();

                Material = new Material(Shader.Find("Custom/Props/Prop/Default"));
                MaterialProperty = new MaterialPropertyBlock();
                ID_Color = Shader.PropertyToID("_Color");

                SingletonMod<Mod>.Logger.Debug("Marker created");
            }
            catch (Exception error)
            {
                SingletonMod<Mod>.Logger.Error("Can't create marker", error);
            }
        }
        public static void DestroyMarker()
        {
            try
            {
                SingletonMod<Mod>.Logger.Debug("Start destroying marker");

                if (Mesh != null)
                {
                    GameObject.Destroy(Mesh);
                    Mesh = null;
                }

                if (Material != null)
                {
                    GameObject.Destroy(Material);
                    Material = null;
                }

                MaterialProperty = null;
                SingletonMod<Mod>.Logger.Debug("Marker destroyed");
            }
            catch (Exception error)
            {
                SingletonMod<Mod>.Logger.Error("Can't destroy marker", error);
            }
        }

        private BuildingSpawnPoint(BuildingData data)
        {
            Data = data;

            Categories = new PropertyULongEnumValue<VehicleCategory>("V", Changed, Data.DefaultVehicles);
            Type = new PropertyEnumValue<PointType>("T", Changed, PointType.Both);
            Position = new PropertyVector4Value(Changed, Vector4.zero, labelY: "H", labelW: "A");
            FollowGround = new PropertyBoolValue("FG", Changed, true);
        }

        public static float GetHeightWithWater(Vector3 position)
        {
            var x = position.x / 16f + 540f;
            var z = position.z / 16f + 540f;

            var xMin = Mathf.Clamp((int)x, 0, 1080);
            var xMax = Mathf.Clamp((int)x + 1, 0, 1080);
            var zMin = Mathf.Clamp((int)z, 0, 1080);
            var zMax = Mathf.Clamp((int)z + 1, 0, 1080);
            var xT = x - (int)x;
            var zT = z - (int)z;
            var cells = WaterCells;

            var zMinHeight = Mathf.Lerp(GetSurfaceHeight(xMin, zMin, cells), GetSurfaceHeight(xMax, zMin, cells), xT);
            var zMaxHeight = Mathf.Lerp(GetSurfaceHeight(xMin, zMax, cells), GetSurfaceHeight(xMax, zMax, cells), xT);
            var height = Mathf.Lerp(zMinHeight, zMaxHeight, zT);
            return height * 0.015625f;

            static float GetSurfaceHeight(int x, int z, WaterSimulation.Cell[] cells)
            {
                var waterHeight = cells[z * 1081 + x].m_height;
                if (waterHeight == 0f)
                    return TerrainManager.instance.FinalHeights[z * 1081 + x];
                if (waterHeight < 64f)
                    return TerrainManager.instance.RawHeights2[z * 1081 + x] + Mathf.Max(0f, waterHeight);
                else
                    return TerrainManager.instance.BlockHeights[z * 1081 + x] + waterHeight;
            }
        }
        public static float GetHeight(Vector3 position)
        {
            var x = position.x / 16f + 540f;
            var z = position.z / 16f + 540f;
            return TerrainManager.instance.SampleFinalHeight(x, z) * 0.015625f;
        }

        public BuildingSpawnPoint(BuildingData data, Vector3 position, float angle = 0f, VehicleCategory vehicleType = VehicleCategory.Default, PointType type = PointType.Both) : this(data)
        {
            Init(position.FixZ(), angle, vehicleType, type);
        }
        public BuildingSpawnPoint(BuildingData data, Vector3 position, Vector3 target, VehicleCategory vehicleType = VehicleCategory.Default, PointType type = PointType.Both, bool invert = false) : this(data)
        {
            position = position.FixZ();
            target = target.FixZ();
            Init(position, (invert ? position - target : target - position).AbsoluteAngle(), vehicleType, type);
        }

        private void Init(Vector4 position, float angle, VehicleCategory vehicleType, PointType type)
        {
            position.w = angle;
            Position.Value = position;
            Categories.Value = vehicleType;
            Type.Value = type;
        }
        private void Changed()
        {
            Services.Clear();
            OnChanged?.Invoke();
        }
        private void AAA()
        {

        }
        public bool IsServiceCorrect(VehicleService service)
        {
            if (!Services.TryGetValue(service, out var pathPos))
            {
                if (!VehicleLaneData.TryGet(service, out var laneData))
                    return true;

                GetAbsolute(ref Data.Id.GetBuilding(), out var absPos, out _);
                PathManager.FindPathPosition(absPos, laneData.Service, laneData.Lane, laneData.Type, laneData.VehicleCategory, false, false, laneData.Distance, false, out pathPos);
                Services[service] = pathPos;
            }

            return pathPos.m_segment != 0;
        }

        public void GetAbsolute(ref Building data, out Vector3 position, out Vector3 target, PositionType type = PositionType.Final)
        {
            position = data.m_position + (Vector3)Position.Value.TurnRad(data.m_angle, false);

            if (FollowGround)
            {
                if (type == PositionType.OnWater)
                    position.y = GetHeightWithWater(position);
                else if (type == PositionType.OnGround)
                    position.y = GetHeight(position);
                else
                    position.y = GetHeightWithWater(position) + Position.Value.y;
            }

            target = position + Vector3.forward.TurnRad(data.m_angle + Position.Value.w * Mathf.Deg2Rad, false);
        }
        public void SetAbsolute(ref Building data, Vector3 position)
        {
            var zeroPos = data.m_position;

            if (FollowGround)
                zeroPos.y = GetHeightWithWater(position);

            var newPos = (position - zeroPos).TurnRad(data.m_angle, true);
            Position.Value = new Vector4(newPos.x, newPos.y, newPos.z, Position.Value.w);
        }

        public BuildingSpawnPoint Copy(BuildingData data = null)
        {
            var copy = new BuildingSpawnPoint(data ?? Data);

            copy.Categories.Value = Categories;
            copy.Type.Value = Type;
            copy.Position.Value = Position;

            return copy;
        }

        public XElement ToXml()
        {
            var config = new XElement(XmlSection);

            Type.ToXml(config);
            Categories.ToXml(config);
            Position.ToXml(config);
            FollowGround.ToXml(config);

            return config;
        }
        public static bool FromXml(Version version, XElement config, BuildingData data, out BuildingSpawnPoint point)
        {
            point = new BuildingSpawnPoint(data);

            point.Type.FromXml(config);
            point.Categories.FromXml(config);
            point.Position.FromXml(config);
            point.FollowGround.FromXml(config);

            if (version < new Version("1.3"))
                point.Categories.Value = point.Categories.Value.GetNew();

            return true;
        }

        public void RenderGeometry(RenderManager.CameraInfo cameraInfo, bool hovered)
        {
            if (Settings.Marker3D)
            {
                GetAbsolute(ref Data.Id.GetBuilding(), out var position, out var target);

                MaterialProperty.Clear();
                MaterialProperty.SetColor(ID_Color, hovered ? Color.white : Color.black);
                var rotation = Quaternion.AngleAxis((target - position).AbsoluteAngle() * Mathf.Rad2Deg, Vector3.down);
                Graphics.DrawMesh(Mesh, position, rotation, Material, Layer, null, 0, MaterialProperty, false, false);
            }
        }
        public void Render(RenderManager.CameraInfo cameraInfo, bool hovered, VehicleCategory type = VehicleCategory.None)
        {
            GetAbsolute(ref Data.Id.GetBuilding(), out var position, out var target);

            if (type != VehicleCategory.None)
            {
                var service = type.GetService();
                if (service != VehicleService.None && VehicleLaneData.TryGet(service, out var laneData))
                {
                    if (PathManager.FindPathPosition(position, laneData.Service, laneData.Lane, laneData.Type, laneData.VehicleCategory, false, false, laneData.Distance, false, out var pathPos))
                    {
                        position.RenderCircle(new OverlayData(cameraInfo) { Width = laneData.Distance * 2f, Color = CommonColors.Blue });
                    }

                    var segment = pathPos.m_segment.GetSegment();
                    var lanes = segment.GetLaneIds().ToArray();
                    if (pathPos.m_lane <= lanes.Length - 1)
                    {
                        var lane = lanes[pathPos.m_lane].GetLane();
                        lane.m_bezier.RenderBezier(new OverlayData(cameraInfo) { Width = segment.Info.m_lanes[pathPos.m_lane].m_width, Color = CommonColors.Blue, Cut = true });

                        var lanePos = lane.m_bezier.Position(pathPos.m_offset / 255f);
                        var connection = new StraightTrajectory(position, lanePos);
                        if (connection.Length >= 3f)
                            connection.Render(new OverlayData(cameraInfo) { Color = CommonColors.Blue });
                    }
                }
            }

            if (hovered)
                position.RenderCircle(new OverlayData(cameraInfo), 2.5f, 1f);

            var color = GetColor();
            position.RenderCircle(new OverlayData(cameraInfo) { Color = color }, 2f, 1.5f);

            var direction = target - position;
            new StraightTrajectory(position + direction, position + direction * 2f).Render(new OverlayData(cameraInfo) { Color = color });

            Color32 GetColor()
            {
                if (Categories == VehicleCategory.None || Type == PointType.None)
                    return CommonColors.Gray192;
                else if (!IsCorrect)
                    return CommonColors.Red;
                else return Type.Value switch
                {
                    PointType.Spawn => CommonColors.Green,
                    PointType.Unspawn => CommonColors.Yellow,
                    PointType.Both => CommonColors.Orange,
                    _ => CommonColors.Gray192,
                };
            }
        }
    }

    public enum PointType
    {
        [NotVisible]
        None = 0,

        [Description(nameof(Localize.PointType_Spawn))]
        Spawn = 1,

        [Description(nameof(Localize.PointType_Unspawn))]
        Unspawn = 2,

        [NotVisible]
        Both = Spawn | Unspawn,
    }
    public enum PositionType
    {
        OnWater,
        OnGround,
        Final
    }
}
