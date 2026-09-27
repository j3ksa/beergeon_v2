using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlackJack
{
    public enum CardSuit
    {
        Hearts,   // Kier ♥ (czerwony)
        Diamonds, // Karo ♦ (czerwony)
        Clubs,    // Trefl ♣ (czarny)
        Spades    // Pik ♠ (czarny)
    }

    public enum CardRank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    [System.Serializable]
    public struct Card
    {
        public CardSuit Suit;
        public CardRank Rank;

        public Card(CardSuit suit, CardRank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public bool IsRed => Suit == CardSuit.Hearts || Suit == CardSuit.Diamonds;

        public string SuitSymbol
        {
            get
            {
                return Suit switch
                {
                    CardSuit.Hearts => "♥",
                    CardSuit.Diamonds => "♦",
                    CardSuit.Clubs => "♣",
                    CardSuit.Spades => "♠",
                    _ => "?"
                };
            }
        }

        public string RankString
        {
            get
            {
                return Rank switch
                {
                    CardRank.Jack => "J",
                    CardRank.Queen => "Q",
                    CardRank.King => "K",
                    CardRank.Ace => "A",
                    _ => ((int)Rank).ToString()
                };
            }
        }

        public int BaseValue
        {
            get
            {
                if (Rank == CardRank.Ace) return 11;
                if (Rank >= CardRank.Ten) return 10;
                return (int)Rank;
            }
        }

        public override string ToString()
        {
            return $"{RankString}{SuitSymbol}";
        }
    }

    public class Deck
    {
        private readonly List<Card> cards = new List<Card>();
        private readonly System.Random rng = new System.Random();

        public int RemainingCards => cards.Count;

        public Deck(int deckCount = 1)
        {
            ResetAndShuffle(deckCount);
        }

        public void ResetAndShuffle(int deckCount = 1)
        {
            cards.Clear();
            for (int d = 0; d < deckCount; d++)
            {
                foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
                {
                    foreach (CardRank rank in Enum.GetValues(typeof(CardRank)))
                    {
                        cards.Add(new Card(suit, rank));
                    }
                }
            }
            Shuffle();
        }

        public void Shuffle()
        {
            // Algorytm Fisher-Yates
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                (cards[i], cards[k]) = (cards[k], cards[i]);
            }
        }

        public Card Draw()
        {
            if (cards.Count == 0)
            {
                ResetAndShuffle();
            }

            Card drawn = cards[0];
            cards.RemoveAt(0);
            return drawn;
        }
    }

    [System.Serializable]
    public class BlackJackHand
    {
        public List<Card> Cards { get; } = new List<Card>();

        public void AddCard(Card card)
        {
            Cards.Add(card);
        }

        public void Clear()
        {
            Cards.Clear();
        }

        /// <summary>
        /// Wylicza łączną wartość ręki. As liczony jako 11, a w razie przekroczenia 21 jako 1.
        /// </summary>
        public int CalculateScore(bool ignoreHiddenDealerCard = false)
        {
            int total = 0;
            int aceCount = 0;

            int count = Cards.Count;
            if (ignoreHiddenDealerCard && count > 1)
            {
                count = 1; // Liczymy tylko pierwszą (odkrytą) kartę krupiera
            }

            for (int i = 0; i < count; i++)
            {
                Card card = Cards[i];
                if (card.Rank == CardRank.Ace)
                {
                    aceCount++;
                    total += 11;
                }
                else
                {
                    total += card.BaseValue;
                }
            }

            while (total > 21 && aceCount > 0)
            {
                total -= 10;
                aceCount--;
            }

            return total;
        }

        public bool IsBust => CalculateScore() > 21;

        public bool IsBlackjack => Cards.Count == 2 && CalculateScore() == 21;
    }
}
