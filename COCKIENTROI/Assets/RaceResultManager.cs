using System.Collections.Generic;
using UnityEngine;

public class RaceResultManager : MonoBehaviour
{
    public static RaceResultManager Instance;

    // Lưu thứ hạng theo thứ tự về đích (store parent GameObject)
    public List<GameObject> ranking = new List<GameObject>();

    // Dùng để truyền dữ liệu sang scene khác (tên object con của parent)
    public List<string> rankingNames = new List<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Lưu GameObject khi về đích và lấy tên con để truyền qua scene khác
    public void AddResult(GameObject racer)
    {
        ranking.Add(racer);

        // Nếu có con, lấy tên con đầu tiên; còn không thì lấy tên parent
        string nameToStore = racer.name;
        if (racer.transform.childCount > 0)
        {
            nameToStore = racer.transform.GetChild(0).name;
        }

        rankingNames.Add(nameToStore);
    }

    // Lấy danh sách kết quả (GameObject)
    public List<GameObject> GetResults()
    {
        return ranking;
    }

    // Lấy danh sách tên để truyền qua scene khác
    public List<string> GetResultNames()
    {
        return rankingNames;
    }

    // Reset khi cần
    public void ClearResults()
    {
        ranking.Clear();
        rankingNames.Clear();
    }
}