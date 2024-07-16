using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XDPaint;

public class SpriteGen : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] GameObject[] Obj;
    public List<GameObject> GeneratedSprite;

    public void GenerateMask()
    {
        if(GeneratedSprite.Count==0)
        {
            GeneratedSprite = new List<GameObject>();
        }
        else
        {
            ClearPrev();
        }
        foreach (var v in Obj)
        {
            GameObject _temp = Instantiate(v, v.transform);
            _temp.transform.localPosition = Vector3.zero;
            _temp.transform.localPosition += new Vector3(1,1,0)*-0.05f;
            _temp.transform.localScale = Vector3.one;
            _temp.GetComponent<SpriteRenderer>().sprite = GetSpr(v.GetComponent<SpriteRenderer>().sprite.name);
            _temp.GetComponent<SpriteRenderer>().sortingOrder = v.GetComponent<SpriteRenderer>().sortingOrder-1;
            var _col = Color.black;
            _col.a = 0.5f;
            _temp.GetComponent<SpriteRenderer>().color = _col;
            GeneratedSprite.Add(_temp);
            DestroyImmediate(_temp.GetComponent<PaintManager>());
        }
    }

    public void ClearPrev()
    {
        foreach (var v in GeneratedSprite)
        {
            DestroyImmediate(v);
        }
        GeneratedSprite.Clear();
    }

    Sprite GetSpr(string name)
    {
        foreach(var v in sprites)
        {
            if(v.name == name)
            {
                return v;
            }
        }
        return null;
    }
}
