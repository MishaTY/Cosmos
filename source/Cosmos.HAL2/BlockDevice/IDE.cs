using System;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.HAL.BlockDevice
{
    public class IDE
    {
        private static PCIDevice xDevice = HAL.PCI.GetDeviceClass(HAL.ClassID.MassStorageController,
                                                                  HAL.SubclassID.IDEInterface);

        internal static void InitDriver()
        {
            if (xDevice != null)
            {
                Console.WriteLine("ATA Primary Master");
                Initialize(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Master);
                Console.WriteLine("ATA Primary Slave");
                Initialize(Ata.ControllerIdEnum.Primary, Ata.BusPositionEnum.Slave);
                Console.WriteLine("ATA Secondary Master");
                Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Master);
                Console.WriteLine("ATA Secondary Slave");
                Initialize(Ata.ControllerIdEnum.Secondary, Ata.BusPositionEnum.Slave);
            }
        }

        private static void Initialize(Ata.ControllerIdEnum aControllerID, Ata.BusPositionEnum aBusPosition)
        {
            var xIO = aControllerID == Ata.ControllerIdEnum.Primary ? Core.Global.BaseIOGroups.ATA1 : Core.Global.BaseIOGroups.ATA2;
            var xATA = new AtaPio(xIO, aControllerID, aBusPosition);
            if (xATA.DriveType == AtaPio.SpecLevel.Null)
                return;
            else if (xATA.DriveType == AtaPio.SpecLevel.ATA)
            {
                BlockDevice.Devices.Add(xATA);
                Ata.AtaDebugger.Send("ATA device with speclevel ATA found.");
            }
            else if (xATA.DriveType == AtaPio.SpecLevel.ATAPI)
            {
                Ata.AtaDebugger.Send("ATA device with speclevel ATAPI found, which is not supported yet!");
                return;
            }

            ScanAndInitPartitons(xATA);
        }

        internal static void ScanAndInitPartitons(BlockDevice device)
        {
            if (GPT.IsGPTPartition(device))
            {
                var xGPT = new GPT(device);

                Ata.AtaDebugger.Send("Number of GPT partitions found:");
                Ata.AtaDebugger.SendNumber(xGPT.Partitions.Count);
                for (int i = 0; i < xGPT.Partitions.Count; i++)
                {
                    var xPart = xGPT.Partitions[i];
                    if (xPart == null)
                    {
                        Console.WriteLine("Null partition found at idx: " + i);
                    }
                    else
                    {
                        var xPartDevice = new Partition(device, xPart.StartSector, xPart.SectorCount);
                        Partition.Partitions.Add(device, xPartDevice);
                        Console.WriteLine("Found partition at idx: " + i);
                    }
                }
            }
            else
            {
                var xMbrData = new byte[512];
                device.ReadBlock(0UL, 1U, ref xMbrData);
                var mbr = new MBR(xMbrData);

                if (mbr.EBRLocation != 0)
                {
                    //EBR Detected
                    var xEbrData = new byte[512];
                    device.ReadBlock(mbr.EBRLocation, 1U, ref xEbrData);
                    var xEBR = new EBR(xEbrData);

                    for (int i = 0; i < xEBR.Partitions.Count; i++)
                    {
                        //var xPart = xEBR.Partitions[i];
                        //var xPartDevice = new BlockDevice.Partition(xATA, xPart.StartSector, xPart.SectorCount);
                        //Partition.Partitions.Add(xATA, xPartDevice);
                    }
                }

                Ata.AtaDebugger.Send("Number of MBR partitions found:");
                Ata.AtaDebugger.SendNumber(mbr.Partitions.Count);
                for (int i = 0; i < mbr.Partitions.Count; i++)
                {
                    var xPart = mbr.Partitions[i];
                    if (xPart == null)
                    {
                        Console.WriteLine("Null partition found at idx: " + i);
                    }
                    else
                    {
                        var xPartDevice = new Partition(device, xPart.StartSector, xPart.SectorCount);
                        Partition.Partitions.Add(device, xPartDevice);
                        Console.WriteLine("Found partition at idx: " + i);
                    }
                }
            }
        }
    }
}
