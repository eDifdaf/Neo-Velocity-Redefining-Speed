using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    [SerializeField] RectTransform CreditsUIElementTransform;
    [SerializeField] float ScrollSpeed;
    void Start()
    {
        CreditsUIElementTransform.position = new Vector3(CreditsUIElementTransform.position.x, -CreditsUIElementTransform.rect.height / 2 + 100);
    }

    void Update()
    {
        CreditsUIElementTransform.position = new Vector3(CreditsUIElementTransform.position.x, CreditsUIElementTransform.position.y + ScrollSpeed * Time.deltaTime);

        if (CreditsUIElementTransform.position.y > CreditsUIElementTransform.rect.height / 2)
            ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Main_Menu");
    }
}
