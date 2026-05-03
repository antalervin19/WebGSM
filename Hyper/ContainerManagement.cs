using System;
using System.Diagnostics;

namespace HyperContainer
{
    public class ContainerLimits
    {
        public int CpuCores { get; set; } = 1;
        public int MemoryMb { get; set; } = 2048;
    }

    public enum PortType
    {
        TCP,
        UDP
    }

    public class ContainerManagement
    {
        private static string Run(string cmd)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + cmd + "\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd() ?? "";
                string error = process.StandardError.ReadToEnd() ?? "";

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                    Console.WriteLine(error);

                return output ?? "";
            }
        }

        // ---------------- CREATE ----------------
        public static void CreateContainer(string id, ContainerLimits limits)
        {
            Console.WriteLine($"Creating container {id}");

            string result = Run($"lxc launch ubuntu:22.04 {id}");

            if (result.Contains("Error") || result.Contains("error"))
            {
                Console.WriteLine("Container creation FAILED");
                return;
            }

            Run($"lxc config set {id} user.owner \"Hyper\"");
            Run($"lxc config set {id} user.type \"HyperContainer\"");
            Run($"lxc config set {id} user.created \"{DateTime.UtcNow:O}\"");

            ApplyLimits(id, limits);
        }

        // ---------------- START / STOP ----------------
        public static void StartContainer(string id) => Run($"lxc start {id}");

        public static void StopContainer(string id) => Run($"lxc stop {id}");

        public static void KillContainer(string id) => Run($"lxc stop {id} --force");

        public static void RestartContainer(string id)
        {
            StopContainer(id);
            StartContainer(id);
        }

        public static void DeleteContainer(string id)
        {
            Run($"lxc delete {id} --force");
        }

        // ---------------- LIMITS ----------------
        private static void ApplyLimits(string id, ContainerLimits limits)
        {
            Run($"lxc config set {id} limits.cpu {limits.CpuCores}");
            Run($"lxc config set {id} limits.memory {limits.MemoryMb}MB");
        }

        // ---------------- PORT MAPPING ----------------
        public static void MapContainerPort(string id, int externalPort, int internalPort, PortType type)
        {
            string protocol = type == PortType.TCP ? "tcp" : "udp";

            Console.WriteLine($"Mapping {protocol.ToUpper()} {externalPort} -> {internalPort} for {id}");

            Run(
                $"lxc config device add {id} port-{externalPort}-{protocol} proxy " +
                $"listen={protocol}:0.0.0.0:{externalPort} " +
                $"connect={protocol}:127.0.0.1:{internalPort}"
            );
        }

        // ---------------- MONITOR ----------------
        public static string MonitorContainer(string id)
        {
            return Run($"lxc exec {id} -- ps aux");
        }

        // ---------------- STATS ----------------
        public static string GetContainerStats(string id)
        {
            string info = Run($"lxc info {id}");
            string ip = Run($"lxc list {id} -c 4 --format csv");

            return $"=== {id} ===\nIP: {ip}\n{info}";
        }

        // ---------------- LIST ----------------
        public static string ListAllContainers()
        {
            return Run("lxc list --format table");
        }

        public static string ListAllContainersJson()
        {
            return Run("lxc list --format json");
        }
    }
}