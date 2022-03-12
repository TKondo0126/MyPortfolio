using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

/**
 *　コリントスクリプト
 */
public class smartballscript : MonoBehaviour {
	GameObject[] ob_cubes;	//障害物オブジェクト
	GameObject[] goals;		//ゴールトリガー

	float power = 0f;		//ボールに与える力
	bool flg = true;		//スペースキー入力フラグ
	public Text score;		//スコア表示

	// Use this for initialization
	void Start () {
		ob_cubes = GameObject.FindGameObjectsWithTag ("ob_cube");	//障害物オブジェクトを取得
		goals = GameObject.FindGameObjectsWithTag ("goal");			//ゴールトリガーを取得
		int n = 0;

		//ゴールトリガーの透過設定
		foreach (GameObject obj in goals) {
			Renderer renderer = obj.GetComponent<Renderer> ();
			renderer.material.SetFloat ("_Mode", 3f);
			renderer.material.SetInt ("_SrcBlend", (int)BlendMode.SrcAlpha);
			renderer.material.SetInt ("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
			renderer.material.SetInt ("_ZWhite", 0);
			renderer.material.DisableKeyword ("_ALPHATEST_ON");
			renderer.material.EnableKeyword ("_ALPHABLEND_ON");
			renderer.material.DisableKeyword ("_ALPHAPREMULTIPLY_ON");
			renderer.material.renderQueue = 3000;
			renderer.material.color = new Color (0f, 0.15f * n, 1f - 0.15f * n++, 0.5f);
		}

		//障害物オブジェクトのアニメーション設定
		foreach (GameObject obj in ob_cubes) {
			Vector3 move = obj.transform.position;
			AnimationClip clip = new AnimationClip ();
			clip.legacy = true;

			//x
			Keyframe[] keysX = new Keyframe[2];
			keysX [0] = new Keyframe (0f, move.x - 6);
			keysX [1] = new Keyframe (1f, move.x + 4);
			AnimationCurve curveX = new AnimationCurve (keysX);
			clip.SetCurve ("", typeof(Transform), "localPosition.x", curveX);
			clip.wrapMode = WrapMode.PingPong;

			//y
			Keyframe[] keysY = new Keyframe[2];
			keysY [0] = new Keyframe (0f, move.y);
			keysY [1] = new Keyframe (1f, move.y);
			AnimationCurve curveY = new AnimationCurve (keysY);
			clip.SetCurve ("", typeof(Transform), "localPosition.y", curveY);

			//z
			Keyframe[] keysZ = new Keyframe[2];
			keysZ [0] = new Keyframe (0f, move.z);
			keysZ [1] = new Keyframe (1f, move.z);
			AnimationCurve curveZ = new AnimationCurve (keysZ);
			clip.SetCurve ("", typeof(Transform), "localPosition.z", curveZ);

			Animation animation = obj.GetComponent<Animation> ();
			animation.AddClip (clip, "clip1");
			animation.Play ("clip1");
		}
	}

	// Update is called once per frame
	void Update () {
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		Renderer renderer = GetComponent<Renderer> ();

		MoveCube ();

		rigidbody.AddForce (0f, 0f, -1f);	//z軸方向へ重力を与える
		//スペースキーは1回のみの効果
		if (flg) {
			if (Input.GetKey (KeyCode.Space)) {
				power += 0.001f;
				if (power > 1f)
					power = 1f;
				renderer.material.color = new Color (1f, power, 0f);	//ボールを徐々に黄色にする
			}
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			rigidbody.AddForce (new Vector3 (0f, 0f, power * 3000f));
			power = 0f;
			renderer.material.color = Color.red;
			flg = false;
		}
	}

	/**　
	 *  障害物を回転させるメソッド
	 *  @args なし
	 *  @return なし
	 */
	void MoveCube(){
		foreach(GameObject obj in ob_cubes)
			obj.transform.Rotate (new Vector3 (0f, 1f, 0f));
	}

	/**
	 * 衝突時のメソッド
	 * @args 衝突された物体
	 * @return なし
	 */
	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "ob_cube") {
			Behaviour b = (Behaviour)collision.gameObject.GetComponent ("Halo");
			b.enabled = true;
		}
	}

	/**
	 * 衝突後離れるときのメソッド
	 * @args 衝突された物体
	 * @return なし
	 */
	void OnCollisionExit(Collision collision){
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		if (collision.gameObject.tag == "ob_cube") {
			Behaviour b = (Behaviour)collision.gameObject.GetComponent ("Halo");
			b.enabled = false;
			Vector3 v = rigidbody.velocity;
			if (v.magnitude < 10) {
				v *= 1.5f;
				if (v.magnitude < 1)
					v *= 1.5f;
				rigidbody.velocity = v;
			}
		}

		if (collision.gameObject.tag == "ob_wall") {
			Vector3 v = rigidbody.velocity;
			if (v.magnitude < 10) {
				v *= 2.0f;
				if (v.magnitude < 1)
					v *= 2.0f;
				rigidbody.velocity = v;
			}
		}
	}

	/**
	 * トリガー接触時のメソッド
	 * @args 接触されたトリガー
	 * @return なし
	 */
	void OnTriggerEnter(Collider collider){
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		if (collider.gameObject.tag == "goal") {
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			int n = 1;

			foreach (GameObject obj in goals) {
				if (obj == collider.gameObject) {
					score.text = "point:" + (n * 100);
					ParticleSystem ps = collider.gameObject.GetComponent<ParticleSystem> ();
					ps.Play ();
				}
				n++;
			}
		}
	}
}
