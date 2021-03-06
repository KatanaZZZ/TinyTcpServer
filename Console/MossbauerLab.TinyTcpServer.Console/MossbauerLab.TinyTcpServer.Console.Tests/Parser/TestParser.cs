﻿using System;
using System.Collections.Generic;
using System.Linq;
using MossbauerLab.TinyTcpServer.Console.Cli.Data;
using MossbauerLab.TinyTcpServer.Console.Cli.Options;
using NUnit.Framework;

namespace MossbauerLab.TinyTcpServer.Console.Tests.Parser
{
    [TestFixture]
    public class TestParser
    {
        [TestCase("--start", "--ipaddr=192.168.111.6", "--port=3999", "--script=EchoScript.cs", "--settings=serverSetting.txt")]
        [TestCase("--StARt", "--IpaddR=192.168.111.6", "   --PORT=3999\t   ", "   --script=EchoScript.cs    ", "\t--SeTtInGs=serverSetting.txt")]
        [TestCase("--stop", null, null, null, null)]
        [TestCase("--StOp", null, null, null, null)]
        [TestCase("--restart", null, null, "--script=FtpScript.cs", "--settings=ftpServerSetting.txt")]
        public void TestSuccessfulParse(String runOption, String ipAddress, String tcpPort, String scriptFile, String settingsFile)
        {
            String[] cmdLineArgs = BuildCmdLine(runOption, ipAddress, tcpPort, scriptFile, settingsFile);
            CommandInfo expectedInfo = BuildCommandInfo(runOption, ipAddress, tcpPort, scriptFile, settingsFile);
            CommandInfo actualInfo = cli.Parser.Parser.Parse(cmdLineArgs);
            CheckCommandInfo(expectedInfo, actualInfo);
        }

        [TestCase("-start", "--ipaddr=192.168.111.6", "--port=3999", "--script=EchoScript.cs", "--settings=serverSetting.txt")]
        [TestCase("start", "--ipaddr=192.168.111.6", "--port=3999", "--script=EchoScript.cs", "--settings=serverSetting.txt")]
        [TestCase("--start", "--ipaddr=192.168.111.6", "--port=3999", "--script=EchoScript.cs", "settings=serverSetting.txt")]
        [TestCase("start", "ipaddr=192.168.111.6", "port=3999", "script=EchoScript.cs", "settings=serverSetting.txt")]
        [TestCase("--start", "--ipaddr=192.168.111.6", "--port=3000", "--script=EchoScript.cs", "--invalidKey=serverSetting.txt")]
        [TestCase("--start", "--ipaddr=192.168.111.6", "--port=3000", "--script=EchoScript.cs", "invalidKey=serverSetting.txt")]
        public void TestParseFailsIncorrectArgsSyntax(String runOption, String ipAddress, String tcpPort, String scriptFile, String settingsFile)
        {
            String[] cmdLineArgs = BuildCmdLine(runOption, ipAddress, tcpPort, scriptFile, settingsFile);
            Assert.Throws<ApplicationException>(() => cli.Parser.Parser.Parse(cmdLineArgs), "checking that parser throws");
        }

        private String[] BuildCmdLine(String option1, String option2, String option3, String option4, String option5)
        {
            IList<String> options = new List<String>();
            if (option1 != null)
                options.Add(option1);
            if (option2 != null)
                options.Add(option2);
            if (option3 != null)
                options.Add(option3);
            if (option4 != null)
                options.Add(option4);
            if (option5 != null)
                options.Add(option5);
            return options.ToArray();
        }

        private CommandInfo BuildCommandInfo(String command, String ipAddress, String tcpPort, String scriptFile, String settingsFile)
        {
            CommandInfo info = new CommandInfo();
            info.Command = _commands[command.ToLower()];
            info.IpAddress = ipAddress != null
                           ? ipAddress.Substring(ipAddress.IndexOf(KeyValueSeparator, StringComparison.CurrentCulture) + 1)
                           : null;
            if(tcpPort != null)
               info.Port = UInt16.Parse(tcpPort.Substring(tcpPort.IndexOf(KeyValueSeparator, StringComparison.CurrentCulture) + 1));
            info.ScriptFile = scriptFile != null
                            ? scriptFile.Substring(scriptFile.IndexOf(KeyValueSeparator, StringComparison.CurrentCulture) + 1)
                            : null;
            info.SettingsFile = settingsFile != null
                              ? settingsFile.Substring(settingsFile.IndexOf(KeyValueSeparator, StringComparison.CurrentCulture) + 1)
                              : null;
            return info;
        }

        private void CheckCommandInfo(CommandInfo expectedInfo, CommandInfo actualInfo)
        {
            Assert.AreEqual(expectedInfo.Command, actualInfo.Command, "Checking that command type are equals");
            Assert.AreEqual(expectedInfo.IpAddress, actualInfo.IpAddress, "Checking that ip address are equals");
            Assert.AreEqual(expectedInfo.Port, actualInfo.Port, "Checking that port are equals");
            Assert.AreEqual(expectedInfo.ScriptFile, actualInfo.ScriptFile, "Checking that script file are equals");
            Assert.AreEqual(expectedInfo.SettingsFile, actualInfo.SettingsFile, "Checking that settings are equals");
        }

        private readonly IDictionary<String, CommandType> _commands = new Dictionary<String, CommandType>()
        {
            {"--start", CommandType.Start},
            {"--stop", CommandType.Stop},
            {"--restart", CommandType.Restart},
            {"--quit", CommandType.Restart},
            {"--help", CommandType.Restart},
        };

        private const String KeyValueSeparator = "=";
    }
}
