﻿//#define COSMOSDEBUG

using System;
using System.Collections.Generic;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem.VFS;

namespace Cosmos.System.FileSystem
{
    public class Disk
    {
        private List<ManagedPartition> parts = new List<ManagedPartition>();
        private static List<FileSystemFactory> registeredFileSystems = new List<FileSystemFactory>();


        /// <summary>
        /// The size of the disk in MB.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// List of partitions
        /// </summary>
        public List<ManagedPartition> Partitions { get { return parts; } }
        /// <summary>
        /// List of file systems.
        /// </summary>
        public static List<FileSystemFactory> RegisteredFileSystemsTypes { get { return registeredFileSystems; } }
        /// <summary>
        /// Main blockdevice that has all of the partitions.
        /// </summary>
        public BlockDevice Host;
        /// <summary>
        /// The Master Boot record (MBR) of the drive.
        /// </summary>
        public byte[] MBR
        {
            get
            {
                var xMBRData = new byte[512];

                Host.ReadBlock(0, 1, ref xMBRData);

                return xMBRData;
            }
            set
            {
                if (value.Length != 512)
                {
                    throw new Exception("MBR must be 512 bytes.");
                }
                Host.WriteBlock(0, 1, ref value);
            }
        }
        public Disk(BlockDevice mainBlockDevice)
        {
            Host = mainBlockDevice;
            if (registeredFileSystems.Count == 0)
            {
                registeredFileSystems.Add(new FatFileSystemFactory());
            }
            foreach (var part in Partition.Partitions)
            {
                if (part.Value == mainBlockDevice)
                {
                    parts.Add(new ManagedPartition(part.Key));
                }
            }
            Size = (int)(mainBlockDevice.BlockCount * mainBlockDevice.BlockSize / 1024 / 1024);
        }
        /// <summary>
        /// Mounts the disk.
        /// </summary>
        public void Mount()
        {
            foreach (var part in Partitions)
            {
                if (part != null)
                {
                    part.Mount();
                }
            }
        }
        /// <summary>
        /// Display information about the disk.
        /// </summary>
        public void DisplayInformation()
        {
            if (Partitions.Count > 0)
            {
                for (int i = 0; i < Partitions.Count; i++)
                {
                    Global.mFileSystemDebugger.SendInternal("Partition #: ");
                    Global.mFileSystemDebugger.SendInternal(i + 1);
                    global::System.Console.WriteLine("Partition #: " + (i + 1));
                    Global.mFileSystemDebugger.SendInternal("Block Size:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockSize);
                    global::System.Console.WriteLine("Block Size: " + Partitions[i].Host.BlockSize + " bytes");
                    Global.mFileSystemDebugger.SendInternal("Block Count:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockCount);
                    global::System.Console.WriteLine("Block Partitions: " + Partitions[i].Host.BlockCount);
                    Global.mFileSystemDebugger.SendInternal("Size:");
                    Global.mFileSystemDebugger.SendInternal(Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize / 1024 / 1024);
                    global::System.Console.WriteLine("Size: " + Partitions[i].Host.BlockCount * Partitions[i].Host.BlockSize / 1024 / 1024 + " MB");
                }
            }
            else
            {
                global::System.Console.WriteLine("No partitions found!");
            }
        }

        /// <summary>
        /// Create Partition.
        /// </summary>
        /// <param name="start">Start.</param>
        /// <param name="end">End.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if start / end is smaller then 0.</exception>
        /// <exception cref="ArgumentException">Thrown if end is smaller or equal to start.</exception>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public void CreatePartion(long start, long end)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (end < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (end <= start)
            {
                throw new ArgumentException("end is <= start");
            }

            throw new NotImplementedException("CreatePartion");
        }
    }
}
