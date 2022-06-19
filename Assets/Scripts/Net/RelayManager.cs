using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
// using Unity.Services.Relay.Apis.Allocations;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
	static private RelayManager _instance = null;
	static public RelayManager Singleton {
		get {
			if( _instance != null )
				return _instance;

			RelayManager[] objs = FindObjectsOfType(typeof(RelayManager)) as RelayManager[];
			Debug.Assert(objs.Length == 1, "There is more than one " + typeof(RelayManager).Name + " in the scene.");

			_instance = objs[0];
			return _instance;
		}
	}

    [SerializeField]
    private string environment = "production";

    [SerializeField]
    private int maxNumberOfConnections = 4;

    public bool IsRelayEnabled => Transport != null && Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

	public string Error = "";

    public async Task<string> SetupRelay()
    {
        Debug.Log($"Relay Server Starting With Max Connections: {maxNumberOfConnections}");

		string joinCode = null;

		try {
	        InitializationOptions options = new InitializationOptions()
	            .SetEnvironmentName(environment);

			await UnityServices.InitializeAsync(options);

			if (!AuthenticationService.Instance.IsSignedIn) {
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}

			Allocation allocation = await Relay.Instance.CreateAllocationAsync(maxNumberOfConnections);

			joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
			string host = allocation.RelayServer.IpV4;
			ushort port = (ushort)allocation.RelayServer.Port;
			byte[] key = allocation.Key;
			byte[] allocationIDBytes = allocation.AllocationIdBytes;
			byte[] connectionData = allocation.ConnectionData;

			Transport.SetRelayServerData(host, port, allocationIDBytes, key, connectionData);
		}
		catch( System.Exception e ) {
			Error = e.Message;
			Debug.Log($"Error: {e.Message}");
			return null;
		}

        Debug.Log($"Relay Server Generated Join Code: {joinCode}");
        return joinCode;
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
		Debug.Log($"Client Joining Game With Join Code: {joinCode}");

		try {
			InitializationOptions options = new InitializationOptions()
				.SetEnvironmentName(environment);

			await UnityServices.InitializeAsync(options);

			if (!AuthenticationService.Instance.IsSignedIn) {
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}

			JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

			string host = allocation.RelayServer.IpV4;
			ushort port = (ushort)allocation.RelayServer.Port;
			byte[] key = allocation.Key;
			byte[] allocationIDBytes = allocation.AllocationIdBytes;
			byte[] connectionData = allocation.ConnectionData;
			byte[] hostConnectionData = allocation.HostConnectionData;

			Transport.SetRelayServerData(host, port, allocationIDBytes, key, connectionData, hostConnectionData);
		}
		catch( System.Exception e ) {
			Debug.Log($"Client Join failed: {e.Message}");
			Error = e.Message;
			return false;
		}

		Debug.Log($"Client Joined Game With Join Code: {joinCode}");
		return true;
    }
}
