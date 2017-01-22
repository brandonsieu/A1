using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using Frontend1;

namespace seng301_asgn1
{
    /// <summary>
    /// Represents the concrete virtual vending machine factory that you will implement.
    /// This implements the IVendingMachineFactory interface, and so all the functions
    /// are already stubbed out for you.
    /// 
    /// Your task will be to replace the TODO statements with actual code.
    /// 
    /// Pay particular attention to extractFromDeliveryChute and unloadVendingMachine:
    /// 
    /// 1. These are different: extractFromDeliveryChute means that you take out the stuff
    /// that has already been dispensed by the machine (e.g. pops, money) -- sometimes
    /// nothing will be dispensed yet; unloadVendingMachine is when you (virtually) open
    /// the thing up, and extract all of the stuff -- the money we've made, the money that's
    /// left over, and the unsold pops.
    /// 
    /// 2. Their return signatures are very particular. You need to adhere to this return
    /// signature to enable good integration with the other piece of code (remember:
    /// this was written by your boss). Right now, they return "empty" things, which is
    /// something you will ultimately need to modify.
    /// 
    /// 3. Each of these return signatures returns typed collections. For a quick primer
    /// on typed collections: https://www.youtube.com/watch?v=WtpoaacjLtI -- if it does not
    /// make sense, you can look up "Generic Collection" tutorials for C#.
    /// </summary>
    public class VendingMachineFactory : IVendingMachineFactory
    {
        List<VendingMachine> VendingMachines = new List<VendingMachine>();

        public VendingMachineFactory()
        {
            // System.Diagnostics.Debug.Write("here");
            // TODO: Implement
        }

        public int createVendingMachine(List<int> coinKinds, int selectionButtonCount)
        {
            // TODO: Implement
            IEnumerable<int> distinct = coinKinds.Distinct();
            if (coinKinds.Count != distinct.Count() || !coinKinds.TrueForAll(isPositive)) throw new InputException();
            VendingMachines.Add(new VendingMachine(coinKinds, selectionButtonCount));
            return VendingMachines.Count - 1;
        }

        public void configureVendingMachine(int vmIndex, List<string> popNames, List<int> popCosts)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            if (!popCosts.TrueForAll(isPositive) || popNames.Count != popCosts.Count) throw new InputException();
            foreach (string popName in popNames) if (popName.Any(c => char.IsDigit(c))) throw new InputException();
            if (popNames.Count != VendingMachines[vmIndex].getSelectionCount()) throw new InputException();
            VendingMachines[vmIndex].config(popNames, popCosts);
            // TODO: Implement
        }

        public void loadCoins(int vmIndex, int coinKindIndex, List<Coin> coins)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            if (coinKindIndex >= VendingMachines[vmIndex].getcoinchuteSize() || coinKindIndex < 0) throw new InputException();
            VendingMachines[vmIndex].pushCoins(coinKindIndex, coins);
            // TODO: Implement
        }

        public void loadPops(int vmIndex, int popKindIndex, List<Pop> pops)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            if (popKindIndex >= VendingMachines[vmIndex].getpopchuteSize() || popKindIndex < 0) throw new InputException();
            VendingMachines[vmIndex].pushPops(popKindIndex, pops);
            // TODO: Implement
        }

        public void insertCoin(int vmIndex, Coin coin)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            if (VendingMachines[vmIndex].analyzeCoins(coin)) VendingMachines[vmIndex].insertCoins(coin);
            // TODO: Implement
        }

        public void pressButton(int vmIndex, int value)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            if (value >= VendingMachines[vmIndex].getpopchuteSize() || value < 0) throw new InputException();
            VendingMachines[vmIndex].processPurchase(value);
            // TODO: Implement
        }

        public List<Deliverable> extractFromDeliveryChute(int vmIndex)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            return VendingMachines[vmIndex].emptyDelivery();
            // TODO: Implement
            //return new List<Deliverable>();
        }

        public List<IList> unloadVendingMachine(int vmIndex)
        {
            if (vmIndex >= VendingMachines.Count || vmIndex < 0) throw new InputException();
            return VendingMachines[vmIndex].unloadMachine();
        }

        private class VendingMachine
        {
            List<Coin> coinCache = new List<Coin>();
            List<Coin> profit = new List<Coin>();
            List<Queue<Coin>> coinChutes = new List<Queue<Coin>>();
            List<Queue<Pop>> popChutes = new List<Queue<Pop>>();
            List<Deliverable> deliveryChute = new List<Deliverable>();
            List<string> popName = new List<string>();
            List<int> popCost = new List<int>();
            List<int> coinValues = new List<int>();
            int selectionButtonNumber;

            public VendingMachine(List<int> coinKinds, int selectionButtonCount)
            {
                coinValues.AddRange(coinKinds);
                selectionButtonNumber = selectionButtonCount;
                for (int i = 0; i < coinValues.Count; i++) coinChutes.Add(new Queue<Coin>());
                for (int j = 0; j < selectionButtonCount; j++) popChutes.Add(new Queue<Pop>());
            }

            public int getSelectionCount()
            {
                return selectionButtonNumber;
            }

            public void config(List<string> popNames, List<int> popCosts)
            {
                popName.AddRange(popNames);
                popCost.AddRange(popCosts);
            }

            public void pushCoins(int coinKindIndex, List<Coin> coins)
            {
                foreach (Coin coin in coins) coinChutes[coinKindIndex].Enqueue(coin);
            }

            public void pushPops(int popKindIndex, List<Pop> pops)
            {
                foreach (Pop pop in pops) popChutes[popKindIndex].Enqueue(pop);
            }

            public int getcoinchuteSize()
            {
                return coinChutes.Count;
            }

            public int getpopchuteSize()
            {
                return popChutes.Count;
            }

            public bool analyzeCoins(Coin coins)
            {
                if (coinValues.Contains(coins.Value)) return true;
                sendtoDelivery(coins);
                return false;
            }

            public void insertCoins(Coin coins)
            {
                coinCache.Add(coins);
            }

            public void processPurchase(int buttonPressed)
            {
                int totalValue = 0;
                foreach (Coin coin in coinCache) totalValue += coin.Value;
                if (totalValue >= popCost[buttonPressed])
                {
                    giveChange(totalValue - popCost[buttonPressed]);
                    givePop(buttonPressed);
                    profit.AddRange(coinCache);
                    coinCache.Clear();
                }
            }

            private void giveChange(int change)
            {
                List<int> coinsLeft = new List<int>();
                coinsLeft.AddRange(coinValues);
                while (coinsLeft.Count != 0)
                {
                    int highest = coinsLeft.Max();
                    int highestIndex = coinsLeft.BinarySearch(highest);
                    if (coinChutes[highestIndex].Count > 0)
                    {
                        int coinCount = (int)(change / highest);
                        int amounttoRemove = Math.Min(coinCount, coinChutes[highestIndex].Count);
                        change -= amounttoRemove * highest;
                        for (int i = amounttoRemove; i > 0; i--) sendtoDelivery(coinChutes[highestIndex].Dequeue());
                    }
                    coinsLeft.Remove(highest);
                }
            }

            private void givePop(int popKindIndex)
            {
                sendtoDelivery(popChutes[popKindIndex].Dequeue());
            }

            private void sendtoDelivery(Deliverable item)
            {
                deliveryChute.Add(item);
            }

            private List<Coin> emptycoinChutes()
            {
                List<Coin> allChange = new List<Coin>();
                foreach (Queue<Coin> chute in coinChutes)
                {
                    foreach (Coin coin in chute)
                    {
                        allChange.Add(coin);
                    }
                }
                return allChange;
            }

            private List<Pop> emptypopChutes()
            {
                List<Pop> allPop = new List<Pop>();
                foreach (Queue<Pop> chute in popChutes)
                {
                    foreach (Pop pop in chute)
                    {
                        allPop.Add(pop);
                    }
                }
                return allPop;
            }

            public List<Deliverable> emptyDelivery()
            {
                List<Deliverable> getDelivery = new List<Deliverable>();
                getDelivery.AddRange(deliveryChute);
                deliveryChute.Clear();
                return getDelivery;
            }

            public List<IList> unloadMachine()
            {
                List<IList> returnedTuple = new List<IList>();
                returnedTuple.Add(emptycoinChutes());
                returnedTuple.Add(profit);
                returnedTuple.Add(emptypopChutes());
                return returnedTuple;
            }

        }
        
        static bool isPositive(int i)
        {
            if (i > 0) return true;
            return false;
        }
    }

    public class InputException : Exception
    {
        public InputException()
        {
        }

        public InputException(string message)
            : base(message)
        {
        }

        public InputException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}