using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector2 suddenForce;

    [SerializeField] Rigidbody2D player;

    bool inputR;
    bool inputLS;

    void Update()
    {
        GatherInput();
        if (inputR)
            ReloadThisLevel();
    }

    void LateUpdate()
    {
        if (inputLS)
            AddSuddenForceToPlayer();
    }

    void GatherInput()
    {
        inputR = Input.GetKeyDown(KeyCode.R);
        inputLS = Input.GetKeyDown(KeyCode.LeftShift);
    }

    void ReloadThisLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void AddSuddenForceToPlayer()
    {
        player.AddForce(new Vector2(suddenForce.x * Mathf.Sign(player.velocity.x), suddenForce.y * Mathf.Sign(player.velocity.y)), ForceMode2D.Impulse);
    }

}
