
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;


public class BoneBusServer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BulkBoneRotation
    {
        public int boneId;
        public float rot_w;
        public float rot_x;
        public float rot_y;
        public float rot_z;

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4}", boneId, rot_w, rot_x, rot_y, rot_z);
        }

        public string ToStringAsEuler()
        {
            Quaternion quat = new Quaternion(rot_x, rot_y, rot_z, rot_w);
            Vector3 euler = quat.eulerAngles;
            return String.Format("{0} {1} {2} {3}", boneId, euler.x.ToString("f4"), euler.y.ToString("f4"), euler.z.ToString("f4"));
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BulkBoneRotations
    {
       public int  packetId;
       public int  time;
       public int  charId;
       public int  numBoneRotations;
       public IntPtr bones;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BulkBonePosition
    {
       public int boneId;
       public float pos_x;
       public float pos_y;
       public float pos_z;

       public override string ToString()
       {
           return String.Format("{0} {1} {2} {3}", boneId, pos_x, pos_y, pos_z);
       }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BulkBonePositions
    {
       public int  packetId;
       public int  time;
       public int  charId;
       public int  numBonePositions;
       public IntPtr bones;
    };


    private const int NETWORK_PORT_UDP = 15100;  // according to Kumar, ICT (unofficially) uses a range in the 15000s (handy unoffical port list: http://www.iana.org/assignments/port-numbers)
    private const int NETWORK_PORT_TCP = 15102;

    private UdpClient m_udpClient;
    private TcpListener m_tcpListener;
    private List<TcpClient> m_tcpClientList;
    private int m_HighestReceivedBulkBoneRotationTime = -1;
    private int m_HighestReceivedBulkBonePositionTime = -1;


    public delegate void OnClientConnectFunc( string clientName, IntPtr userData );
    public delegate void OnClientDisconnectFunc();
    public delegate void OnCreateCharacterFunc( int characterID, string characterType, string characterName, int skeletonType, IntPtr userData);
    public delegate void OnUpdateCharacterFunc(int characterID, string characterType, string characterName, int skeletonType, IntPtr userData);
    public delegate void OnDeleteCharacterFunc(int characterID, IntPtr userData);
    public delegate void OnSetCharacterPositionFunc(int characterID, float x, float y, float z, IntPtr userData);
    public delegate void OnSetCharacterRotationFunc(int characterID, float w, float x, float y, float z, IntPtr userData);
    public delegate void OnBoneRotationsFunc(ref BulkBoneRotations bulkBoneRotations, IntPtr userData);
    public delegate void OnBonePositionsFunc(ref BulkBonePositions bulkBonePositions, IntPtr userData);
    public delegate void OnSetCharacterVisemeFunc(int characterID, int visemeId, float weight, float blendTime, IntPtr userData);
    public delegate void OnSetBoneIdFunc(int characterID, string boneName, int boneId, IntPtr userData);
    public delegate void OnSetVisemeIdFunc(int characterID, string visemeName, int visemeId, IntPtr userData);
    public delegate void OnPlaySoundFunc(string soundFile, string characterName, IntPtr userData);
    public delegate void OnStopSoundFunc(string soundFile, IntPtr userData);
    public delegate void OnExecScriptFunc(string command, IntPtr userData);


    public OnClientConnectFunc m_OnClientConnectFunc;
    public OnClientDisconnectFunc m_OnClientDisconnectFunc;// { add {} remove {} }
    public OnCreateCharacterFunc m_OnCreateCharacterFunc;
    public OnUpdateCharacterFunc m_OnUpdateCharacterFunc;
    public OnDeleteCharacterFunc m_OnDeleteCharacterFunc;
    public OnSetCharacterPositionFunc m_OnSetCharacterPositionFunc;
    public OnSetCharacterRotationFunc m_OnSetCharacterRotationFunc;
    public OnBoneRotationsFunc m_OnBoneRotationsFunc;
    public OnBonePositionsFunc m_OnBonePositionsFunc;
    public OnSetCharacterVisemeFunc m_OnSetCharacterVisemeFunc;
    public OnSetBoneIdFunc m_OnSetBoneIdFunc;
    public OnSetVisemeIdFunc m_OnSetVisemeIdFunc;
    public OnPlaySoundFunc m_OnPlaySoundFunc;
    public OnStopSoundFunc m_OnStopSoundFunc;
    public OnExecScriptFunc m_OnExecScriptFunc;// { add {} remove {} }


    public BoneBusServer()
    {
    }


    public bool OpenConnection()
    {
        m_udpClient = new UdpClient( NETWORK_PORT_UDP );

        m_tcpClientList = new List<TcpClient>();
        m_tcpListener = new TcpListener( IPAddress.Any, NETWORK_PORT_TCP );
        m_tcpListener.Start();

        return true;
    }

    public bool CloseConnection()
    {
        m_udpClient.Close();
        for (int i = 0; i < m_tcpClientList.Count; i++)
        {
            m_tcpClientList[i].Close();
        }
        m_tcpListener.Stop();

        return true;
    }


    public bool Update()
    {
        //if ( !IsOpen() )
        //{
        //   return false;
        //}

        //Debug.Log( "BoneBus.Update() - Start" );


        if ( m_tcpListener.Pending() )
        {
            TcpClient client = m_tcpListener.AcceptTcpClient();
            m_tcpClientList.Add( client );

            Debug.Log( "TCP Client Connected: " + m_tcpClientList.Count.ToString() );

            string clientIP = ((IPEndPoint)(client.Client.RemoteEndPoint)).Address.ToString();

            if ( m_OnClientConnectFunc != null )
                m_OnClientConnectFunc( clientIP, new IntPtr() );
        }


        for ( int i = 0; i < m_tcpClientList.Count; i++ )
        {
            //Debug.Log( "BoneBus.Update() - TCP" );

            TcpClient tcpClient = m_tcpClientList[ i ];

            NetworkStream stream = tcpClient.GetStream();

            if ( stream.DataAvailable )
            {
                byte [] tcpBuffer = new byte[ 2048 ];

                string strBuffer = "";

                int bytesReceived = stream.Read( tcpBuffer, 0, tcpBuffer.Length );
                while ( bytesReceived > 0 )
                {
                    //Debug.Log( "TCP: " + bytesReceived.ToString() + " - '" + strBuffer + "'" );

                    strBuffer += System.Text.Encoding.ASCII.GetString(tcpBuffer, 0, bytesReceived);

                    if ( stream.DataAvailable )
                        bytesReceived = stream.Read( tcpBuffer, 0, tcpBuffer.Length );
                    else
                        bytesReceived = 0;
                }

                if ( strBuffer.Length > 0 )
                {
                    //Debug.Log( "TCP: '" + strBuffer );

                    string [] tokens = strBuffer.Split( ';' );

                    for ( int t = 0; t < tokens.Length; t++ )
                    {
                        string [] msgTokens = tokens[ t ].Split( '|' );

                        if ( msgTokens.Length > 0 )
                        {
                            if ( msgTokens[ 0 ] == "CreateActor" )
                            {
                                if ( msgTokens.Length > 4 )
                                {
                                    string charIdStr = msgTokens[ 1 ];
                                    int charId       = Convert.ToInt32( charIdStr );
                                    string uClass    = msgTokens[ 2 ];
                                    string name      = msgTokens[ 3 ];
                                    int skeletonType = Convert.ToInt32( msgTokens[ 4 ] );

                                    Debug.Log( "TCP: '" + strBuffer );

                                    if ( m_OnCreateCharacterFunc != null )
                                        m_OnCreateCharacterFunc( charId, uClass, name, skeletonType, new IntPtr() );
                                }
                            }
                            else if (msgTokens[0] == "UpdateActor")
                            {
                                if (msgTokens.Length > 4)
                                {
                                    string charIdStr = msgTokens[1];
                                    int charId = Convert.ToInt32(charIdStr);
                                    string uClass = msgTokens[2];
                                    string name = msgTokens[3];
                                    int skeletonType = Convert.ToInt32(msgTokens[4]); ;

                                    if (m_OnUpdateCharacterFunc != null)
                                    {
                                        m_OnUpdateCharacterFunc(charId, uClass, name, skeletonType, new IntPtr());
                                    }
                                }
                            }
                            else if ( msgTokens[ 0 ] == "DeleteActor" )
                            {
                                if ( msgTokens.Length > 1 )
                                {
                                    string charIdStr = msgTokens[ 1 ];
                                    int charId       = Convert.ToInt32( charIdStr );

                                    if ( m_OnDeleteCharacterFunc != null )
                                        m_OnDeleteCharacterFunc( charId, new IntPtr() );
                                }
                            }
                            else if ( msgTokens[ 0 ] == "SetParamNameMap" )
                            {
                                if ( msgTokens.Length == 3 )
                                {
                                    //string charIdStr = msgTokens[ 1 ];
                                    //const char * paramName = charIdStr.c_str();
                                    //int paramNameId = Convert.ToInt32( msgTokens[ 2 ] );

                                    //m_OnSetCharacterParamFunc( paramName, paramNameId, m_onSetCharacterParamUserData );
                                }
                            }
                            else if ( msgTokens[ 0 ] == "SetActorPos" )
                            {
                                if ( msgTokens.Length > 4 )
                                {
                                    string charIdStr = msgTokens[ 1 ];
                                    int charId       = Convert.ToInt32( charIdStr );
                                    float x          = Convert.ToSingle( msgTokens[ 2 ] );
                                    float y          = Convert.ToSingle( msgTokens[ 3 ] );
                                    float z          = Convert.ToSingle( msgTokens[ 4 ] );

                                    if ( m_OnSetCharacterPositionFunc != null )
                                        m_OnSetCharacterPositionFunc( charId, x, y, z, new IntPtr() );
                                }
                            }
                            else if ( msgTokens[ 0 ] == "SetActorRot" )
                            {
                                if ( msgTokens.Length > 5 )
                                {
                                    string charIdStr = msgTokens[ 1 ];
                                    int charId       = Convert.ToInt32( charIdStr );
                                    float w          = Convert.ToSingle( msgTokens[ 2 ] );
                                    float x          = Convert.ToSingle( msgTokens[ 3 ] );
                                    float y          = Convert.ToSingle( msgTokens[ 4 ] );
                                    float z          = Convert.ToSingle( msgTokens[ 5 ] );

                                    if ( m_OnSetCharacterRotationFunc != null )
                                        m_OnSetCharacterRotationFunc( charId, w, x, y, z, new IntPtr() );
                                }
                            }
                            else if ( msgTokens[ 0 ] == "SetActorViseme" )
                            {
                                if ( msgTokens.Length > 4 )
                                {
                                    string charIdStr = msgTokens[ 1 ];
                                    int charId       = Convert.ToInt32( charIdStr );
                                    string visemeStr = msgTokens[ 2 ];
                                    int visemeId     = Convert.ToInt32( visemeStr );
                                    float weight     = Convert.ToSingle( msgTokens[ 3 ] );
                                    float blendTime  = Convert.ToSingle( msgTokens[ 4 ] );

                                    if ( m_OnSetCharacterVisemeFunc != null )
                                        m_OnSetCharacterVisemeFunc( charId, visemeId, weight, blendTime, new IntPtr() );
                                }
                            }
                            else if (msgTokens[0] == "SetBoneId")
                            {
                                if (msgTokens.Length > 3)
                                {
                                    string charIdStr = msgTokens[1];
                                    int charId = Convert.ToInt32(charIdStr);
                                    string boneName = msgTokens[2];
                                    int boneId = Convert.ToInt32(msgTokens[3]);

                                    //Debug.Log("charId: " + charId + " boneName: " + boneName + " boneId: " + boneId);

                                    if (m_OnSetBoneIdFunc != null)
                                    {
                                        m_OnSetBoneIdFunc(charId, boneName, boneId, new IntPtr());
                                    }
                                }
                            }
                            else if (msgTokens[0] == "SetVisemeId")
                            {
                                if (msgTokens.Length > 3)
                                {
                                    string charIdStr = msgTokens[1];
                                    int charId = Convert.ToInt32(charIdStr);
                                    string visemeName = msgTokens[2];
                                    int visemeId = Convert.ToInt32(msgTokens[3]);

                                    if (m_OnSetVisemeIdFunc != null)
                                    {
                                        m_OnSetVisemeIdFunc(charId, visemeName, visemeId, new IntPtr());
                                    }
                                }
                            }
                            else if ( msgTokens[ 0 ] == "PlaySoundDynamic" )
                            {
                                if ( msgTokens.Length > 2 )
                                {
                                    string soundFile = msgTokens[ 1 ];
                                    string charName  = msgTokens[ 2 ];

                                    if ( m_OnPlaySoundFunc != null )
                                        m_OnPlaySoundFunc( soundFile, charName, new IntPtr() );
                                }
                            }
                            else if ( msgTokens[ 0 ] == "StopSoundDynamic" )
                            {
                                if ( msgTokens.Length > 1 )
                                {
                                    string soundFile = msgTokens[ 1 ];

                                    if ( m_OnStopSoundFunc != null )
                                        m_OnStopSoundFunc( soundFile, new IntPtr() );
                                }
                            }
                        }
                    }
                }
            }
        }


        byte [] buffer = new byte [1];

        while ( buffer.Length > 0 )
        {
            buffer = new byte[0];

            if ( m_udpClient.Available > 0 )
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                buffer = m_udpClient.Receive( ref remoteEP );
            }

            if ( buffer.Length > 3 )
            {
                int opcode =  BitConverter.ToInt32( buffer, 0 );

                //Debug.Log( "BoneBus.Update() - UDP: " + opcode.ToString() );

                if ( opcode == 0x10 )
                {
                    //Debug.Log( "BULK_BONE_DATA: " + buffer.Length );

                    GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();

                    BulkBoneRotations bulkBoneRotations;
                    bulkBoneRotations.packetId = BitConverter.ToInt32( buffer, 0 );
                    bulkBoneRotations.time = BitConverter.ToInt32(buffer, 4);
                    bulkBoneRotations.charId = BitConverter.ToInt32( buffer, 8 );
                    bulkBoneRotations.numBoneRotations = BitConverter.ToInt32( buffer, 12 );
                    bulkBoneRotations.bones = pointer.Increment( 16 );

                    if (m_OnBoneRotationsFunc != null && bulkBoneRotations.time >= m_HighestReceivedBulkBoneRotationTime)
                    {
                        m_HighestReceivedBulkBoneRotationTime = bulkBoneRotations.time;
                        m_OnBoneRotationsFunc(ref bulkBoneRotations, new IntPtr());
                    }

                    pinnedArray.Free();
                }
                else if ( opcode == 0x11 )
                {
                    //Debug.Log( "BULK_POSITION_DATA: " + buffer.Length );

                    GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    IntPtr pointer = pinnedArray.AddrOfPinnedObject();

                    BulkBonePositions bulkBonePositions;
                    bulkBonePositions.packetId = BitConverter.ToInt32( buffer, 0 );
                    bulkBonePositions.time = BitConverter.ToInt32(buffer, 4);
                    bulkBonePositions.charId = BitConverter.ToInt32( buffer, 8 );
                    bulkBonePositions.numBonePositions = BitConverter.ToInt32( buffer, 12 );
                    bulkBonePositions.bones = pointer.Increment( 16 );

                    if (m_OnBonePositionsFunc != null && bulkBonePositions.time >= m_HighestReceivedBulkBonePositionTime)
                    {
                        m_HighestReceivedBulkBonePositionTime = bulkBonePositions.time;
                        m_OnBonePositionsFunc(ref bulkBonePositions, new IntPtr());
                    }

                    pinnedArray.Free();
                }
                else if ( opcode == 0x04 )
                {
                    //Debug.Log( "ExecScriptData: " + buffer.Length );

                    /*
                    ExecScriptData * execScriptData;

                    execScriptData = (ExecScriptData *)buffer;

                    if ( m_onExecScriptFunc )
                    {
                        m_onExecScriptFunc( execScriptData->command, m_onExecScriptUserData );
                    }
                    */
                }
                else if ( opcode == 0x12)
                {
                    //Debug.Log( "BulkGeneralParams: " + buffer.Length );

                    /*
                    BulkGeneralParams * bulkGeneralParams;
                    bulkGeneralParams = (BulkGeneralParams *)buffer;
                    if ( m_onGeneralParamFunc )
                    {
                        m_onGeneralParamFunc( bulkGeneralParams, m_onGeneralParamUserData );
                    }
                    */
                }
            }
        }

        return true;
    }
}
