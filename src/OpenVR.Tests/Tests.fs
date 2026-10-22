namespace Valve.VR.Tests

open System
open Aardvark.Base
open System.Reflection
open Expecto
open Expecto.Logging
open Valve.VR

module Tests =

    type private VRSystem(typ) =
        do
            IntrospectionProperties.CustomEntryAssembly <- Assembly.GetExecutingAssembly()
            Aardvark.Init()

        let system =
            if not <| OpenVR.IsRuntimeInstalled() then
                skiptest "No OpenVR runtime installed"

            let mutable err = EVRInitError.None
            let sys = OpenVR.Init(&err, typ)
            if err <> EVRInitError.None then
                failwithf "Init() failed: %s" (OpenVR.GetStringForHmdError err)
            sys

        member _.System = system

        member _.GetStringTrackedDeviceProperty(deviceIndex, property) =
            let builder = System.Text.StringBuilder(4096, 4096)
            let mutable err = ETrackedPropertyError.TrackedProp_Success
            let len = system.GetStringTrackedDeviceProperty(deviceIndex, property, builder, uint32 builder.Capacity, &err)
            Expect.equal err ETrackedPropertyError.TrackedProp_Success $"Failed to get property {property}"
            builder.ToString()

        member _.Dispose() = OpenVR.Shutdown()

        interface IDisposable with
            member this.Dispose() = this.Dispose()

    module private Cases =

        let initAndShutdown() =
            use _ = new VRSystem(EVRApplicationType.VRApplication_Utility)
            ()

        let queryDevices() =
            use s = new VRSystem(EVRApplicationType.VRApplication_Scene)

            let mutable deviceCount = 0
            let logger = Log.create "Devices"

            for i in 0u .. OpenVR.k_unMaxTrackedDeviceCount - 1u do
                let t = s.System.GetTrackedDeviceClass i
                if t <> ETrackedDeviceClass.Invalid then
                    let vendor = s.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_ManufacturerName_String)
                    let model = s.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_ModelNumber_String)
                    logger.info (
                        Message.eventX $"Device {i}" >>
                        Message.setField "Type" t >>
                        Message.setField "Vendor" vendor >>
                        Message.setField "Model" model
                    )
                    deviceCount <- deviceCount + 1

            Expect.isGreaterThan deviceCount 0 "No devices found"

    [<Tests>]
    let tests =
        [
            "Init and Shutdown", Cases.initAndShutdown
            "Query devices",     Cases.queryDevices
        ]
        |> List.map (fun (name, run) -> testCase name run |> testSequenced)
        |> testList "Tests"