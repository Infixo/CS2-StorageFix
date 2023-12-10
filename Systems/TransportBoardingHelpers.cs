using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game.City;
using Game.Companies;
using Game.Economy;
using Game.Prefabs;
using Game.Routes;
using Game.Vehicles;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Game;
using Game.Simulation;

namespace MonitorMod.Systems;

public static class TransportBoardingHelpers
{
    public struct BoardingLookupData
    {
        [ReadOnly]
        public ComponentLookup<TransportLine> m_TransportLineData;

        [ReadOnly]
        public ComponentLookup<Connected> m_ConnectedData;

        [ReadOnly]
        public ComponentLookup<Game.Routes.TransportStop> m_TransportStopData;

        [ReadOnly]
        public ComponentLookup<PrefabRef> m_PrefabRefData;

        [ReadOnly]
        public ComponentLookup<TransportLineData> m_PrefabTransportLineData;

        [ReadOnly]
        public ComponentLookup<CargoTransportVehicleData> m_PrefabCargoTransportVehicleData;

        [ReadOnly]
        public ComponentLookup<Train> m_TrainData;

        [ReadOnly]
        public ComponentLookup<Aircraft> m_AircraftData;

        [ReadOnly]
        public ComponentLookup<Watercraft> m_WatercraftData;

        [ReadOnly]
        public ComponentLookup<Game.Vehicles.DeliveryTruck> m_DeliveryTruckData;

        [ReadOnly]
        public BufferLookup<LayoutElement> m_LayoutElements;

        [ReadOnly]
        public BufferLookup<RouteModifier> m_RouteModifiers;

        public ComponentLookup<Game.Vehicles.CargoTransport> m_CargoTransportData;

        public ComponentLookup<Game.Vehicles.PublicTransport> m_PublicTransportData;

        public ComponentLookup<BoardingVehicle> m_BoardingVehicleData;

        public ComponentLookup<VehicleTiming> m_VehicleTimingData;

        public BufferLookup<StorageTransferRequest> m_StorageTransferRequests;

        public BufferLookup<Resources> m_EconomyResources;

        public BufferLookup<LoadingResources> m_LoadingResources;

        public BoardingLookupData(SystemBase system)
        {
            m_TransportLineData = system.GetComponentLookup<TransportLine>(isReadOnly: true);
            m_ConnectedData = system.GetComponentLookup<Connected>(isReadOnly: true);
            m_TransportStopData = system.GetComponentLookup<Game.Routes.TransportStop>(isReadOnly: true);
            m_PrefabRefData = system.GetComponentLookup<PrefabRef>(isReadOnly: true);
            m_PrefabTransportLineData = system.GetComponentLookup<TransportLineData>(isReadOnly: true);
            m_PrefabCargoTransportVehicleData = system.GetComponentLookup<CargoTransportVehicleData>(isReadOnly: true);
            m_LayoutElements = system.GetBufferLookup<LayoutElement>(isReadOnly: true);
            m_RouteModifiers = system.GetBufferLookup<RouteModifier>(isReadOnly: true);
            m_CargoTransportData = system.GetComponentLookup<Game.Vehicles.CargoTransport>();
            m_PublicTransportData = system.GetComponentLookup<Game.Vehicles.PublicTransport>();
            m_BoardingVehicleData = system.GetComponentLookup<BoardingVehicle>();
            m_VehicleTimingData = system.GetComponentLookup<VehicleTiming>();
            m_StorageTransferRequests = system.GetBufferLookup<StorageTransferRequest>();
            m_EconomyResources = system.GetBufferLookup<Resources>();
            m_LoadingResources = system.GetBufferLookup<LoadingResources>();
            m_DeliveryTruckData = system.GetComponentLookup<Game.Vehicles.DeliveryTruck>(isReadOnly: true);
            m_AircraftData = system.GetComponentLookup<Aircraft>(isReadOnly: true);
            m_WatercraftData = system.GetComponentLookup<Watercraft>(isReadOnly: true);
            m_TrainData = system.GetComponentLookup<Train>(isReadOnly: true);
        }

        public void Update(SystemBase system)
        {
            m_TransportLineData.Update(system);
            m_ConnectedData.Update(system);
            m_TransportStopData.Update(system);
            m_PrefabRefData.Update(system);
            m_PrefabTransportLineData.Update(system);
            m_PrefabCargoTransportVehicleData.Update(system);
            m_LayoutElements.Update(system);
            m_RouteModifiers.Update(system);
            m_CargoTransportData.Update(system);
            m_PublicTransportData.Update(system);
            m_BoardingVehicleData.Update(system);
            m_VehicleTimingData.Update(system);
            m_StorageTransferRequests.Update(system);
            m_EconomyResources.Update(system);
            m_LoadingResources.Update(system);
            m_DeliveryTruckData.Update(system);
            m_AircraftData.Update(system);
            m_WatercraftData.Update(system);
            m_TrainData.Update(system);
        }
    }

    public struct BoardingData
    {
        public struct Concurrent
        {
            private NativeQueue<BoardingItem>.ParallelWriter m_BoardingQueue;

            public Concurrent(BoardingData data)
            {
                m_BoardingQueue = data.m_BoardingQueue.AsParallelWriter();
            }

            public void BeginBoarding(Entity vehicle, Entity route, Entity stop, Entity waypoint, Entity currentStation, Entity nextStation, bool refuel)
            {
                BoardingItem value = default(BoardingItem);
                value.m_Begin = true;
                value.m_Refuel = refuel;
                value.m_Testing = false;
                value.m_Vehicle = vehicle;
                value.m_Route = route;
                value.m_Stop = stop;
                value.m_Waypoint = waypoint;
                value.m_CurrentStation = currentStation;
                value.m_NextStation = nextStation;
                m_BoardingQueue.Enqueue(value);
            }

            public void EndBoarding(Entity vehicle, Entity route, Entity stop, Entity waypoint, Entity currentStation, Entity nextStation)
            {
                BoardingItem value = default(BoardingItem);
                value.m_Begin = false;
                value.m_Refuel = false;
                value.m_Testing = false;
                value.m_Vehicle = vehicle;
                value.m_Route = route;
                value.m_Stop = stop;
                value.m_Waypoint = waypoint;
                value.m_CurrentStation = currentStation;
                value.m_NextStation = nextStation;
                m_BoardingQueue.Enqueue(value);
            }

            public void BeginTesting(Entity vehicle, Entity route, Entity stop, Entity waypoint)
            {
                BoardingItem value = default(BoardingItem);
                value.m_Begin = true;
                value.m_Refuel = false;
                value.m_Testing = true;
                value.m_Vehicle = vehicle;
                value.m_Route = route;
                value.m_Stop = stop;
                value.m_Waypoint = waypoint;
                value.m_CurrentStation = Entity.Null;
                value.m_NextStation = Entity.Null;
                m_BoardingQueue.Enqueue(value);
            }

            public void EndTesting(Entity vehicle, Entity route, Entity stop, Entity waypoint)
            {
                BoardingItem value = default(BoardingItem);
                value.m_Begin = false;
                value.m_Refuel = false;
                value.m_Testing = true;
                value.m_Vehicle = vehicle;
                value.m_Route = route;
                value.m_Stop = stop;
                value.m_Waypoint = waypoint;
                value.m_CurrentStation = Entity.Null;
                value.m_NextStation = Entity.Null;
                m_BoardingQueue.Enqueue(value);
            }
        }

        private NativeQueue<BoardingItem> m_BoardingQueue;

        public BoardingData(Allocator allocator)
        {
            m_BoardingQueue = new NativeQueue<BoardingItem>(allocator);
        }

        public void Dispose()
        {
            m_BoardingQueue.Dispose();
        }

        public void Dispose(JobHandle inputDeps)
        {
            m_BoardingQueue.Dispose(inputDeps);
        }

        public JobHandle ScheduleBoarding(SystemBase system, CityStatisticsSystem statsSystem, BoardingLookupData lookupData, uint simulationFrameIndex, JobHandle inputDeps)
        {
            TransportBoardingJob jobData = default(TransportBoardingJob);
            jobData.m_SimulationFrameIndex = simulationFrameIndex;
            jobData.m_BoardingLookupData = lookupData;
            jobData.m_BoardingQueue = m_BoardingQueue;
            jobData.m_StatisticsEventQueue = statsSystem.GetStatisticsEventQueue(out var deps).AsParallelWriter();
            JobHandle jobHandle = IJobExtensions.Schedule(jobData, JobHandle.CombineDependencies(inputDeps, deps));
            statsSystem.AddWriter(jobHandle);
            return jobHandle;
        }

        public Concurrent ToConcurrent()
        {
            return new Concurrent(this);
        }
    }

    private struct BoardingItem
    {
        public bool m_Begin;

        public bool m_Refuel;

        public bool m_Testing;

        public Entity m_Vehicle;

        public Entity m_Route;

        public Entity m_Stop;

        public Entity m_Waypoint;

        public Entity m_CurrentStation;

        public Entity m_NextStation;
    }

    [BurstCompile]
    private struct TransportBoardingJob : IJob
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct LoadingResourceComparer : IComparer<LoadingResources>
        {
            public int Compare(LoadingResources x, LoadingResources y)
            {
                return x.m_Amount - y.m_Amount;
            }
        }

        [ReadOnly]
        public uint m_SimulationFrameIndex;

        public BoardingLookupData m_BoardingLookupData;

        public NativeQueue<BoardingItem> m_BoardingQueue;

        public NativeQueue<StatisticsEvent>.ParallelWriter m_StatisticsEventQueue;

        public void Execute()
        {
            BoardingItem item;
            while (m_BoardingQueue.TryDequeue(out item))
            {
                if (item.m_Testing)
                {
                    if (item.m_Begin)
                    {
                        BeginTesting(item);
                    }
                    else
                    {
                        EndTesting(item);
                    }
                }
                else if (item.m_Begin)
                {
                    BeginBoarding(item);
                }
                else
                {
                    EndBoarding(item);
                }
            }
        }

        private void BeginTesting(BoardingItem data)
        {
            BoardingVehicle value = m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop];
            if (!(value.m_Testing != Entity.Null) || !(value.m_Testing != data.m_Vehicle) || ((!m_BoardingLookupData.m_CargoTransportData.TryGetComponent(value.m_Testing, out var componentData) || (componentData.m_State & CargoTransportFlags.Testing) == 0) && (!m_BoardingLookupData.m_PublicTransportData.TryGetComponent(value.m_Testing, out var componentData2) || (componentData2.m_State & PublicTransportFlags.Testing) == 0)))
            {
                value.m_Testing = data.m_Vehicle;
                m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop] = value;
                if (m_BoardingLookupData.m_CargoTransportData.HasComponent(data.m_Vehicle))
                {
                    Game.Vehicles.CargoTransport value2 = m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle];
                    value2.m_State &= ~CargoTransportFlags.RequireStop;
                    value2.m_State |= CargoTransportFlags.Testing;
                    m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle] = value2;
                }
                if (m_BoardingLookupData.m_PublicTransportData.HasComponent(data.m_Vehicle))
                {
                    Game.Vehicles.PublicTransport value3 = m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle];
                    value3.m_State &= ~PublicTransportFlags.RequireStop;
                    value3.m_State |= PublicTransportFlags.Testing;
                    m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle] = value3;
                }
            }
        }

        private void EndTesting(BoardingItem data)
        {
            BoardingVehicle value = m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop];
            if (value.m_Testing == data.m_Vehicle)
            {
                value.m_Testing = Entity.Null;
                m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop] = value;
            }
            if (m_BoardingLookupData.m_CargoTransportData.HasComponent(data.m_Vehicle))
            {
                Game.Vehicles.CargoTransport value2 = m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle];
                value2.m_State &= ~CargoTransportFlags.Testing;
                m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle] = value2;
            }
            if (m_BoardingLookupData.m_PublicTransportData.HasComponent(data.m_Vehicle))
            {
                Game.Vehicles.PublicTransport value3 = m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle];
                value3.m_State &= ~PublicTransportFlags.Testing;
                m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle] = value3;
            }
        }

        private void BeginBoarding(BoardingItem data)
        {
            BoardingVehicle value = m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop];
            if (value.m_Vehicle != Entity.Null && value.m_Vehicle != data.m_Vehicle && ((m_BoardingLookupData.m_CargoTransportData.TryGetComponent(value.m_Vehicle, out var componentData) && (componentData.m_State & CargoTransportFlags.Boarding) != 0) || (m_BoardingLookupData.m_PublicTransportData.TryGetComponent(value.m_Vehicle, out var componentData2) && (componentData2.m_State & PublicTransportFlags.Boarding) != 0)))
            {
                return;
            }
            //Plugin.Log($"BeginBoarding: Train_{data.m_Vehicle.Index} from {data.m_CurrentStation.Index} to {data.m_NextStation.Index}");
            PrefabRef prefabRef = m_BoardingLookupData.m_PrefabRefData[data.m_Route];
            TransportLine transportLine = m_BoardingLookupData.m_TransportLineData[data.m_Route];
            VehicleTiming value2 = m_BoardingLookupData.m_VehicleTimingData[data.m_Waypoint];
            Connected connected = m_BoardingLookupData.m_ConnectedData[data.m_Waypoint];
            DynamicBuffer<RouteModifier> routeModifiers = m_BoardingLookupData.m_RouteModifiers[data.m_Route];
            Game.Vehicles.CargoTransport value3 = default(Game.Vehicles.CargoTransport);
            Game.Vehicles.PublicTransport value4 = default(Game.Vehicles.PublicTransport);
            uint departureFrame = 0u;
            if (m_BoardingLookupData.m_CargoTransportData.HasComponent(data.m_Vehicle))
            {
                value3 = m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle];
                departureFrame = value3.m_DepartureFrame;
            }
            if (m_BoardingLookupData.m_PublicTransportData.HasComponent(data.m_Vehicle))
            {
                value4 = m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle];
                departureFrame = value4.m_DepartureFrame;
            }
            TransportLineData prefabLineData = m_BoardingLookupData.m_PrefabTransportLineData[prefabRef.m_Prefab];
            float targetStopTime = ((!m_BoardingLookupData.m_TransportStopData.HasComponent(connected.m_Connected)) ? prefabLineData.m_StopDuration : RouteUtils.GetStopDuration(prefabLineData, m_BoardingLookupData.m_TransportStopData[connected.m_Connected]));
            value.m_Vehicle = data.m_Vehicle;
            value2.m_AverageTravelTime = RouteUtils.UpdateAverageTravelTime(value2.m_AverageTravelTime, departureFrame, m_SimulationFrameIndex);
            departureFrame = (((value3.m_State & CargoTransportFlags.EnRoute) == 0 && (value4.m_State & PublicTransportFlags.EnRoute) == 0) ? (m_SimulationFrameIndex + 60) : (value2.m_LastDepartureFrame = RouteUtils.CalculateDepartureFrame(transportLine, prefabLineData, routeModifiers, targetStopTime, value2.m_LastDepartureFrame, m_SimulationFrameIndex)));
            m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop] = value;
            m_BoardingLookupData.m_VehicleTimingData[data.m_Waypoint] = value2;
            if (m_BoardingLookupData.m_CargoTransportData.HasComponent(data.m_Vehicle))
            {
                value3.m_State |= CargoTransportFlags.Boarding;
                if (data.m_Refuel)
                {
                    value3.m_State |= CargoTransportFlags.Refueling;
                }
                value3.m_DepartureFrame = departureFrame;
                m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle] = value3;
            }
            if (m_BoardingLookupData.m_PublicTransportData.HasComponent(data.m_Vehicle))
            {
                value4.m_State |= PublicTransportFlags.Boarding;
                if (data.m_Refuel)
                {
                    value4.m_State |= PublicTransportFlags.Refueling;
                }
                value4.m_DepartureFrame = departureFrame;
                m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle] = value4;
            }
            DynamicBuffer<LoadingResources> loadingResources = default(DynamicBuffer<LoadingResources>);
            if (m_BoardingLookupData.m_LoadingResources.HasBuffer(data.m_Vehicle))
            {
                loadingResources = m_BoardingLookupData.m_LoadingResources[data.m_Vehicle];
                loadingResources.Clear();
            }
            UnloadResources(data.m_Vehicle, data.m_CurrentStation);
            LoadResources(data.m_Vehicle, data.m_CurrentStation, data.m_NextStation, loadingResources);
        }

        private void EndBoarding(BoardingItem data)
        {
            BoardingVehicle value = m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop];
            if (value.m_Vehicle == data.m_Vehicle)
            {
                value.m_Vehicle = Entity.Null;
                m_BoardingLookupData.m_BoardingVehicleData[data.m_Stop] = value;
                LoadResources(data.m_Vehicle, data.m_CurrentStation, data.m_NextStation, default(DynamicBuffer<LoadingResources>));
            }
            if (m_BoardingLookupData.m_CargoTransportData.HasComponent(data.m_Vehicle))
            {
                Game.Vehicles.CargoTransport value2 = m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle];
                value2.m_State &= ~(CargoTransportFlags.Boarding | CargoTransportFlags.Refueling);
                m_BoardingLookupData.m_CargoTransportData[data.m_Vehicle] = value2;
            }
            if (m_BoardingLookupData.m_PublicTransportData.HasComponent(data.m_Vehicle))
            {
                Game.Vehicles.PublicTransport value3 = m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle];
                value3.m_State &= ~(PublicTransportFlags.Boarding | PublicTransportFlags.Refueling);
                m_BoardingLookupData.m_PublicTransportData[data.m_Vehicle] = value3;
            }
        }

        private void UnloadResources(Entity vehicle, Entity target) // in BeginBoarding, target == current station
        {
            if (!m_BoardingLookupData.m_EconomyResources.HasBuffer(target))
            {
                return;
            }
            //Plugin.Log($"UnloadRes3 from Train_{vehicle.Index} to {target.Index}");
            DynamicBuffer<Resources> targetResources = m_BoardingLookupData.m_EconomyResources[target];

            // Infixo FIX FOR NEGATIVE RESOURCES
            Plugin.Log($"Checking resources at {target.Index} while Unloading");
            ResourceIterator iterator = ResourceIterator.GetIterator();
            while (iterator.Next())
            {
                int currentResources = EconomyUtils.GetResources(iterator.resource, targetResources);
                if (currentResources < 0)
                {
                    Plugin.Log($"...Fixing negative {iterator.resource} at {target.Index}");
                    EconomyUtils.SetResources(iterator.resource, targetResources, 0);
                }
            }

            if (m_BoardingLookupData.m_LayoutElements.HasBuffer(vehicle))
            {
                DynamicBuffer<LayoutElement> dynamicBuffer = m_BoardingLookupData.m_LayoutElements[vehicle];
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    Entity vehicle2 = dynamicBuffer[i].m_Vehicle;
                    if (m_BoardingLookupData.m_EconomyResources.HasBuffer(vehicle2))
                    {
                        DynamicBuffer<Resources> sourceResources = m_BoardingLookupData.m_EconomyResources[vehicle2];
                        UnloadResources(sourceResources, targetResources);
                    }
                }
            }
            else if (m_BoardingLookupData.m_EconomyResources.HasBuffer(vehicle))
            {
                DynamicBuffer<Resources> sourceResources2 = m_BoardingLookupData.m_EconomyResources[vehicle];
                UnloadResources(sourceResources2, targetResources);
            }
        }

        private void UnloadResources(DynamicBuffer<Resources> sourceResources, DynamicBuffer<Resources> targetResources)
        {
            //Plugin.Log($"UnloadRes2: {sourceResources.Length}");
            for (int i = 0; i < sourceResources.Length; i++)
            {
                Resources resources = sourceResources[i];
                EconomyUtils.AddResources(resources.m_Resource, resources.m_Amount, targetResources);
            }
            sourceResources.Clear();
        }

        private void LoadResources(Entity vehicle, Entity source, Entity target, DynamicBuffer<LoadingResources> loadingResources)
        {
            if (m_BoardingLookupData.m_EconomyResources.HasBuffer(source) && m_BoardingLookupData.m_StorageTransferRequests.HasBuffer(source))
            {
                DynamicBuffer<Resources> resources = m_BoardingLookupData.m_EconomyResources[source];
                DynamicBuffer<StorageTransferRequest> dynamicBuffer = m_BoardingLookupData.m_StorageTransferRequests[source];
                //Plugin.Log($"LoadRes3: from {source.Index} onto Train_{vehicle.Index} to {target.Index} STR {dynamicBuffer.Length}");
                //for (int i = 0; i < dynamicBuffer.Length; i++)
                //{
                //StorageTransferRequest str = dynamicBuffer[i];
                //if (str.m_Target == target) Plugin.Log($"LoadRes3[{i}]: {str.m_Amount} of {str.m_Resource} to {str.m_Target.Index} {str.m_Flags}");
                //}

                // Infixo FIX FOR NEGATIVE RESOURCES
                /*
                Plugin.Log($"Checking resources at {source.Index} while Loading");
                ResourceIterator iterator = ResourceIterator.GetIterator();
                while (iterator.Next())
                {
                    int currentResources = EconomyUtils.GetResources(iterator.resource, resources);
                    if (currentResources < 0)
                    {
                        Plugin.Log($"...Fixing negative {iterator.resource} at {source.Index}");
                        EconomyUtils.SetResources(iterator.resource, resources, 0);
                    }
                }
                */
                int num = 0;
                while (num < dynamicBuffer.Length)
                {
                    StorageTransferRequest value = dynamicBuffer[num];
                    if ((value.m_Flags & StorageTransferFlags.Incoming) != 0 || (value.m_Flags & StorageTransferFlags.Transport) == 0 || value.m_Target != target)
                    {
                        num++;
                        continue;
                    }
                    // Infixo FIX FOR NEGATIVE RESOURCES
                    int currentResources = EconomyUtils.GetResources(value.m_Resource, resources);
                    if (currentResources < 0)
                    {
                        Plugin.Log($"Fixing negative {value.m_Resource} at {source.Index}...");
                        EconomyUtils.SetResources(value.m_Resource, resources, 0);
                        Plugin.Log($"...and removing impossible transfer request for {value.m_Amount} of {value.m_Resource} at {source.Index} (we have 0)");
                        dynamicBuffer.RemoveAt(num);
                        continue;
                    }
                    // Infixo Fixes for erroneous STRs
                    if (currentResources == 0)
                    {
                        Plugin.Log($"Removing impossible transfer request for {value.m_Amount} of {value.m_Resource} at {source.Index} (we have 0)");
                        dynamicBuffer.RemoveAt(num);
                        continue;
                    }
                    if (value.m_Amount > currentResources)
                    {
                        Plugin.Log($"Tweaking impossible transfer request for {value.m_Amount} of {value.m_Resource} at {source.Index} - we have {currentResources} - setting to half");
                        value.m_Amount = currentResources >> 1;
                    }
                    if (value.m_Amount > 0)
                    {
                        // Infixo: check if is possible
                        //int curRes = EconomyUtils.GetResources(value.m_Resource, resources);
                        int amount = value.m_Amount;
                        LoadResources(value.m_Resource, ref value.m_Amount, vehicle);
                        if (value.m_Amount < amount)
                        {
                            // HERE WE TAKE THE RESOURCES
                            //Plugin.Log($"LoadRes4: {value.m_Resource}, amount {amount} -> {value.m_Amount}, storage {curRes}");
                            EconomyUtils.AddResources(value.m_Resource, value.m_Amount - amount, resources);
                        }
                    }
                    if (value.m_Amount == 0)
                    {
                        dynamicBuffer.RemoveAt(num);
                        continue;
                    }
                    dynamicBuffer[num++] = value;
                    if (!loadingResources.IsCreated)
                    {
                        continue;
                    }
                    for (int i = 0; i < loadingResources.Length; i++)
                    {
                        LoadingResources value2 = loadingResources[i];
                        if (value2.m_Resource == value.m_Resource)
                        {
                            value2.m_Amount += value.m_Amount;
                            loadingResources[i] = value2;
                            value.m_Amount = 0;
                            break;
                        }
                    }
                    if (value.m_Amount != 0)
                    {
                        loadingResources.Add(new LoadingResources
                        {
                            m_Resource = value.m_Resource,
                            m_Amount = value.m_Amount
                        });
                    }
                }
            }
            if (loadingResources.IsCreated && loadingResources.Length >= 2)
            {
                loadingResources.AsNativeArray().Sort(default(LoadingResourceComparer));
            }
        }

        private void LoadResources(Resource resource, ref int amount, Entity vehicle)
        {
            //Plugin.Log($"LoadRes2: {amount} of {resource} onto Train_{vehicle.Index}");
            if (m_BoardingLookupData.m_LayoutElements.HasBuffer(vehicle))
            {
                DynamicBuffer<LayoutElement> dynamicBuffer = m_BoardingLookupData.m_LayoutElements[vehicle];
                for (int i = 0; i < dynamicBuffer.Length; i++)
                {
                    Entity vehicle2 = dynamicBuffer[i].m_Vehicle;
                    if (m_BoardingLookupData.m_EconomyResources.HasBuffer(vehicle2))
                    {
                        PrefabRef prefabRef = m_BoardingLookupData.m_PrefabRefData[vehicle2];
                        CargoTransportVehicleData vehicleData = m_BoardingLookupData.m_PrefabCargoTransportVehicleData[prefabRef.m_Prefab];
                        DynamicBuffer<Resources> targetResources = m_BoardingLookupData.m_EconomyResources[vehicle2];
                        TransportType vehicleType = GetVehicleType(vehicle2);
                        LoadResources(resource, ref amount, vehicleData, targetResources, vehicleType);
                        if (amount == 0)
                        {
                            break;
                        }
                    }
                }
            }
            else if (m_BoardingLookupData.m_EconomyResources.HasBuffer(vehicle))
            {
                PrefabRef prefabRef2 = m_BoardingLookupData.m_PrefabRefData[vehicle];
                CargoTransportVehicleData vehicleData2 = m_BoardingLookupData.m_PrefabCargoTransportVehicleData[prefabRef2.m_Prefab];
                DynamicBuffer<Resources> targetResources2 = m_BoardingLookupData.m_EconomyResources[vehicle];
                TransportType vehicleType2 = GetVehicleType(vehicle);
                LoadResources(resource, ref amount, vehicleData2, targetResources2, vehicleType2);
            }
        }

        private void LoadResources(Resource resource, ref int amount, CargoTransportVehicleData vehicleData, DynamicBuffer<Resources> targetResources, TransportType transportType)
        {
            int num = vehicleData.m_CargoCapacity;
            int num2 = -1;
            for (int i = 0; i < targetResources.Length; i++)
            {
                Resources resources = targetResources[i];
                num -= resources.m_Amount;
                num2 = math.select(num2, i, resources.m_Resource == resource);
            }
            int num3 = math.min(num, amount);
            if (num3 == 0)
            {
                return;
            }
            //Plugin.Log($"LoadRes1: {amount} of {resource} onto {transportType} (capacity {vehicleData.m_CargoCapacity}).");
            if (num2 >= 0)
            {
                Resources value = targetResources[num2];
                value.m_Amount += num3;
                targetResources[num2] = value;
            }
            else
            {
                if (targetResources.Length >= vehicleData.m_MaxResourceCount || (vehicleData.m_Resources & resource) == Resource.NoResource)
                {
                    return;
                }
                Resources elem = default(Resources);
                elem.m_Resource = resource;
                elem.m_Amount = num3;
                targetResources.Add(elem);
            }
            amount -= num3;
            switch (transportType)
            {
            case TransportType.Train:
                m_StatisticsEventQueue.Enqueue(new StatisticsEvent
                {
                    m_Statistic = StatisticType.CargoCountTrain,
                    m_Change = num3
                });
                break;
            case TransportType.Ship:
                m_StatisticsEventQueue.Enqueue(new StatisticsEvent
                {
                    m_Statistic = StatisticType.CargoCountShip,
                    m_Change = num3
                });
                break;
            case TransportType.Airplane:
                m_StatisticsEventQueue.Enqueue(new StatisticsEvent
                {
                    m_Statistic = StatisticType.CargoCountAirplane,
                    m_Change = num3
                });
                break;
            }
        }

        private TransportType GetVehicleType(Entity vehicle)
        {
            if (m_BoardingLookupData.m_AircraftData.HasComponent(vehicle))
            {
                return TransportType.Airplane;
            }
            if (m_BoardingLookupData.m_TrainData.HasComponent(vehicle))
            {
                return TransportType.Train;
            }
            if (m_BoardingLookupData.m_WatercraftData.HasComponent(vehicle))
            {
                return TransportType.Ship;
            }
            if (m_BoardingLookupData.m_DeliveryTruckData.HasComponent(vehicle))
            {
                return TransportType.Bus;
            }
            return TransportType.None;
        }
    }
}
