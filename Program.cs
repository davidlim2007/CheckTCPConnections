using System;
using System.Threading;
using System.Net.NetworkInformation;

namespace CheckTCPConnections
{
    // This code is based on the code provided in :
    // In C#, how to check if a TCP port is available?
    // https://stackoverflow.com/questions/570098/in-c-how-to-check-if-a-tcp-port-is-available
    // 
    class Program
    {
        // GetCurrentTCPConnections() returns the latest array of TcpConnectionInformation instances.
        static TcpConnectionInformation[] GetCurrentTCPConnections()
        {
            // Obtain the current system tcp connections. This is the same information provided
            // by the netstat command line application.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            return tcpConnInfoArray;
        }

        // ConnectionExists() checks a TcpConnectionInformation object to see if an equivalent 
        // object (i.e. with the same LocalEndPoint and RemoteEndPoint values) exists in
        // an array of TcpConnectionInformation objects.
        static bool ConnectionExists(TcpConnectionInformation tcpConnInfo, TcpConnectionInformation[] tcpConnInfoArray)
        {
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if
                (
                    (tcpConnInfo.LocalEndPoint.Address.Equals(tcpi.LocalEndPoint.Address))
                    &&
                    (tcpConnInfo.LocalEndPoint.Port.Equals(tcpi.LocalEndPoint.Port))
                    &&
                    (tcpConnInfo.RemoteEndPoint.Address.Equals(tcpi.RemoteEndPoint.Address))
                    &&
                    (tcpConnInfo.RemoteEndPoint.Port.Equals(tcpi.RemoteEndPoint.Port))
                )
                {
                    return true;
                }
            }

            return false;
        }

        // CheckNewTCPConnections() checks for new TCP/IP connections.
        // It does this by :
        //
        // 1. Comparing the latest array of TcpConnectionInformation instances
        // against the base line TcpConnectionInformation instances.
        //
        // 2. Any instance in the latest with values not found in the base line 
        // is singled out and is considered a New Connection.
        //
        // 3. The New Connection instance is stored in an array which is returned.
        //
        static TcpConnectionInformation[] CheckNewTCPConnections(TcpConnectionInformation[] tcpBaseLineConnInfoArray, TcpConnectionInformation[] tcpLatestConnInfoArray)
        {            
            TcpConnectionInformation[] tcpConnInfoArrayRet = new TcpConnectionInformation[0];

            foreach (TcpConnectionInformation tcpci in tcpLatestConnInfoArray)
            {
                if (ConnectionExists(tcpci, tcpBaseLineConnInfoArray) == false)
                {
                    // Add new connection to tcpConnInfoArrayRet.
                    int iCurrentLength = tcpConnInfoArrayRet.Length;
                    Array.Resize(ref tcpConnInfoArrayRet, iCurrentLength + 1);
                    tcpConnInfoArrayRet[iCurrentLength] = tcpci;
                }
            }

            return tcpConnInfoArrayRet;
        }

        // CheckRemovedTCPConnections() checks for TCP/IP connections which have been removed.
        // It does this by :
        //
        // 1. Comparing the base line array of TcpConnectionInformation instances
        // against the latest TcpConnectionInformation instances.
        //
        // 2. Any instance in the base line with values not found in the latest
        // is singled out and is considered a Deleted Connection.
        //
        // 3. The Deleted Connection instance is stored in an array which is returned.
        //
        static TcpConnectionInformation[] CheckRemovedTCPConnections(TcpConnectionInformation[] tcpBaseLineConnInfoArray, TcpConnectionInformation[] tcpLatestConnInfoArray)
        {
            TcpConnectionInformation[] tcpConnInfoArrayRet = new TcpConnectionInformation[0];

            foreach (TcpConnectionInformation tcpci in tcpBaseLineConnInfoArray)
            {
                if (ConnectionExists(tcpci, tcpLatestConnInfoArray) == false)
                {
                    // Add deleted connection to tcpConnInfoArrayRet.
                    int iCurrentLength = tcpConnInfoArrayRet.Length;
                    Array.Resize(ref tcpConnInfoArrayRet, iCurrentLength + 1);
                    tcpConnInfoArrayRet[iCurrentLength] = tcpci;
                }
            }

            return tcpConnInfoArrayRet;            
        }

        static void Main(string[] args)
        {
            // Get the latest TCP connections and use it as the Base Line.
            m_baseLineTCPConnInfoArray = GetCurrentTCPConnections();

            while (true)
            {
                // At the start of every loop, get the latest TCP Connections
                // and consider this the Current Connections.
                TcpConnectionInformation[] tcpCurrentConnInfoArray = GetCurrentTCPConnections();

                // Compare Base Line with the Current and get the set of New Connections.
                TcpConnectionInformation[] tcpNewConnInfoArray = CheckNewTCPConnections(m_baseLineTCPConnInfoArray, tcpCurrentConnInfoArray);

                // Display the list of New Connections.
                foreach (TcpConnectionInformation tcpci in tcpNewConnInfoArray)
                {
                    Console.WriteLine("Connection Created {0:S}:{1:D} {2:S}:{3:D}",
                        tcpci.LocalEndPoint.Address.ToString(), tcpci.LocalEndPoint.Port,
                        tcpci.RemoteEndPoint.Address.ToString(), tcpci.RemoteEndPoint.Port);
                }

                // Compare Base Line with the Current and get the set of Deleted Connections.
                TcpConnectionInformation[] tcpRemovedConnInfoArray = CheckRemovedTCPConnections(m_baseLineTCPConnInfoArray, tcpCurrentConnInfoArray);

                // Display the list of Deleted Connections.
                foreach (TcpConnectionInformation tcpci in tcpRemovedConnInfoArray)
                {
                    Console.WriteLine("Connection Deleted {0:S}:{1:D} {2:S}:{3:D}",
                        tcpci.LocalEndPoint.Address.ToString(), tcpci.LocalEndPoint.Port,
                        tcpci.RemoteEndPoint.Address.ToString(), tcpci.RemoteEndPoint.Port);
                }

                // Refresh m_baseLineTCPConnInfoArray with the Current Connections.
                m_baseLineTCPConnInfoArray = tcpCurrentConnInfoArray;

                Thread.Sleep(500);
            }
        }

        private static TcpConnectionInformation[] m_baseLineTCPConnInfoArray;
    }
}
