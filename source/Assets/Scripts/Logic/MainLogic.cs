using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UtopiaLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MainLogic : MonoBehaviour {

	public string api_version = "1.0";
	public string api_host = "http://127.0.0.1";
	public int api_port = 22824;
	public string last_error = "";

	public InputField api_tokenField;
	public InputField api_portField;
	public Text errorTextObject;
	//public string mainSceneName = "Playground";
	public GameObject mainPanel;
	public GameObject hidableObjects;
	public GameObject mainUI;
	public Text versionInfoLabel;
	public Text balanceLabel;
	public RawImage userAvatarImage;
	public Text hashedPKLabel;

	public Image imageIndicatorCRP;
	public Image imageIndicatorMining;
	public Image imageIndicatorChannels;

	protected string api_token = "";
	protected UtopiaLib.Client client;

	public void showError(string error_info = "") {
		if (errorTextObject) {
			errorTextObject.text = error_info;
		} else {
			Debug.Log ("errorTextObject not specified");
			Debug.Log ("error catched: " + error_info);
		}
	}

	public void actionAuth() {
		if(!api_tokenField) {
			showError ("token field not specified");
		}

		this.api_token = api_tokenField.text;
		int.TryParse (api_portField.text, out api_port);
		if(!auth()) {
			showError (last_error);
		}

		//SceneManager.LoadScene (mainSceneName);
		mainPanel.SetActive (false);
		hidableObjects.SetActive (false);
		mainUI.SetActive (true);

		loadClientData ();
	}

	bool auth() {
		try {
			client = new UtopiaLib.Client(api_host, api_port, api_token);
		} catch(UtopiaLib.ApiErrorException apiex) {
			last_error = "api error: " + apiex.Message;
			return false;
		} catch(SocketException socketex) {
			//TODO: solve problem with socket exception uncaught (wtf??)
			last_error = "socket exception: " + socketex.Message;
			return false;
		} catch(System.Exception ex) {
			last_error = "catched exception: " + ex.Message;
			return false;
		}

		return client.checkClientConnection ();
	}

	void loadClientData() {
		//load utopia client version
		JObject systemInfo = client.getSystemInfo ();
		versionInfoLabel.text = "Utopia client v." + systemInfo ["build_number"].ToString();

		//get account public key
		JObject account_data = client.getOwnContact ();
		string account_pubkey = account_data ["pk"].ToString ();
		hashedPKLabel.text = "hashed pk: " + account_data ["hashedPk"].ToString ();

		//get account avatar
		string avatar_base64 = client.getContactAvatar (account_pubkey);
		byte[] avatar_bytes = System.Convert.FromBase64String(avatar_base64);
		Texture2D avatar_texture = new Texture2D(1, 1);
		avatar_texture.LoadImage( avatar_bytes );
		userAvatarImage.texture = avatar_texture;

		//load account balance
		//TODO: check getBalance method available for this token
		float balance = (float) client.getBalance ();
		//float balance_truncated = Mathf.Floor (balance * 100) / 100;
		balanceLabel.text = "Balance: " + balance.ToString () + " CRP";

		//indicators & network info
		JObject network_info = client.getNetworkConnections();
		setIndicatorStatus (imageIndicatorCRP, (bool) network_info["summary"]["crypton_engine_status"]);
		setIndicatorStatus (imageIndicatorChannels, ! (bool) network_info["summary"]["channel_database_sync_status"]);
		//TOQ: not sure
		int mining_status = client.statusHistoryMining ();
		setIndicatorStatus (imageIndicatorMining, mining_status == 1);
	}

	void setIndicatorStatus(Image indicator_image, bool is_active = true) {
		Color indicator_color;
		if (is_active) {
			indicator_color = Color.green;
		} else {
			indicator_color = Color.red;
		}

		if (!indicator_image) {
			Debug.Log ("indicator image not specified");
		} else {
			indicator_image.color = indicator_color;
		}
	}
}
