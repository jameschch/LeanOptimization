using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using QuantConnect.Indicators;
using System;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Algorithm;
using QuantConnect.Orders;
using QuantConnect.Data.Market;
using QuantConnect.Securities;
using QuantConnect.Configuration;

namespace QuantConnect 
{
	/*
    *   QuantConnect University: 50-10 EMA - Exponential Moving Average Cross
    *   
    *   The classic exponential moving average cross implementation using a custom class.
    *   The algorithm uses allows for a safety margin so you can reduce the "bounciness" 
    *   of the trading to confirm the crossing.
    */
	public enum TRADETYPE
	{
		LOSING,
		NEUTRAL,
		WINNING
	};

	public class EMATest : QCAlgorithm 
	{ 
		//Define required variables:
		int quantity = 0;
		decimal price = 0;
		decimal tolerance = 0m; //0.1% safety margin in prices to avoid bouncing.
		string symbol = "SPY";
		DateTime sampledToday = DateTime.Now;
		List<decimal> _tradeReturns = new List<decimal>();
		private const int MAXRETURNS = 4;
		//Set up the EMA Class:
		ExponentialMovingAverage emaShort;
		ExponentialMovingAverage emaLong;
		private DateTime startTime;

		//Initialize the data and resolution you require for your strategy:
		public override void Initialize() 
		{          
			SetStartDate(2000, 01, 01);
			SetEndDate(2004,01,01);  
			SetCash(25000);
			AddSecurity(SecurityType.Equity, symbol, Resolution.Daily);
			var shortval = Config.GetInt ("EMA_VAR1",10);
			var longval = Config.GetInt ("EMA_VAR2",50);
			emaShort = EMA(symbol, shortval, Resolution.Daily);
			emaLong = EMA(symbol, longval, Resolution.Daily);
			for (int i = 0; i < MAXRETURNS; i++) {
				_tradeReturns.Add(0);
			}
			startTime = DateTime.Now;
		}
		public override void OnOrderEvent(OrderEvent orderEvent)
		{
			if (orderEvent.Status == OrderStatus.Filled && orderEvent.FillQuantity < 0)
			{
				SecurityHolding s = Securities [orderEvent.Symbol].Holdings;	
				var profit_pct = s.LastTradeProfit / Portfolio.TotalPortfolioValue;
				_tradeReturns.Add (profit_pct);
				if (_tradeReturns.Count > MAXRETURNS)
					_tradeReturns.RemoveAt (0);
			}
		}
		public override void OnEndOfAlgorithm()
		{
			Log(string.Format("\nAlgorithm Name: {0}\n Symbol: {1}\n Ending Portfolio Value: {2} \n Start Time: {3}\n End Time: {4}", this.GetType().Name, symbol, Portfolio.TotalPortfolioValue, startTime, DateTime.Now));
			#region logging
			#endregion
		}


		//Handle TradeBar Events: a TradeBar occurs on every time-interval
		public void OnData(TradeBars data) {

			//One data point per day:
			if (sampledToday.Date == data[symbol].Time.Date) return;

			//Only take one data point per day (opening price)
			price = Securities[symbol].Close;
			sampledToday = data[symbol].Time;

			//Wait until EMA's are ready:
			if (!emaShort.IsReady || !emaLong.IsReady) return;

			//Get fresh cash balance: Set purchase quantity to equivalent 10% of portfolio.
			decimal cash = Portfolio.Cash;
			int holdings = Portfolio[symbol].Quantity;
			//quantity = Convert.ToInt32((cash * 0.5m) / price);

			if (holdings > 0) {
				//If we're long, or flat: check if EMA crossed negative: and crossed outside our safety margin:
				if ((emaShort * (1+tolerance)) < emaLong) 
				{
					//Now go short: Short-EMA signals a negative turn: reverse holdings
					Order(symbol, -(holdings));
					Log(Time.ToShortDateString() + " > Go Short > Holdings: " + holdings.ToString() + " Quantity:" + quantity.ToString() + " Samples: " + emaShort.Samples);
				}

			} else if (holdings == 0) {
				//If we're short, or flat: check if EMA crossed positive: and crossed outside our safety margin:
				if ((emaShort * (1 - tolerance)) > emaLong) 
				{
					//Now go long: Short-EMA crossed above long-EMA by sufficient margin
					var quantity = GetNumSymbols(price);
					Order(symbol, Math.Abs(holdings) + quantity);
					Log(Time.ToShortDateString() + "> Go Long >  Holdings: " + holdings.ToString() + " Quantity:" + quantity.ToString() + " Samples: " + emaShort.Samples); 
				}
			}
		}
		private TRADETYPE PredictNextTrade()
		{
			var res = TRADETYPE.WINNING;
			if (_tradeReturns.Count == 4) {
				HiddenMarkovModel hmm = new HiddenMarkovModel(states: 3, symbols: 3);
				int[] observationSequence = GetSequence (_tradeReturns);
				BaumWelchLearning teacher = new BaumWelchLearning(hmm);

				// and call its Run method to start learning
				double error = teacher.Run(observationSequence);
				int[] predict = hmm.Predict (observationSequence, 1);
				if (predict [0] == 0) {
					res = TRADETYPE.LOSING;
				} else if (predict [0] == 1) {
					res = TRADETYPE.NEUTRAL;
				} else if (predict [0] == 2) {
					res = TRADETYPE.WINNING;
				}


			}
			return res;
		}
		private int[] GetSequence(List<decimal> returns)
		{

			int[] observationSequence = new int[4];
			for(int i=0;i<returns.Count;i++)
			{
				if (returns [i] < 0m)
					observationSequence [i] = 0; // loss
				else if (returns [i] > 0m && returns [i] < 0.01m)
					observationSequence [i] = 1;  //neutral, small win
				else 
					observationSequence [i] = 2; //big win
			};
			return observationSequence;
		}
		private int GetNumSymbols(decimal currentPrice)
		{
			var direction = PredictNextTrade ();
			var predict_risk = 1.0m;
			switch (direction) {
			case TRADETYPE.LOSING:
				predict_risk = 0.5m;
				break;
			case TRADETYPE.NEUTRAL:
				predict_risk = 0.8m;
				break;

			}
			var quantity = (int)(Math.Round (predict_risk* Portfolio.Cash/currentPrice));
			return quantity;
		}




	}
}