using System.Collections.Generic;

interface ICardContainer
{
    List<UnityEngine.GameObject> CardList
    {
        get;
    }

    public void AddCard(UnityEngine.GameObject card);

    public void RemoveCard(UnityEngine.GameObject card);
}
