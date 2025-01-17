﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using NUnit.Framework;
using System.Threading;
using QuantConnect.Brokerages.Kraken;
using QuantConnect.Data;
using QuantConnect.Logging;
using QuantConnect.Data.Market;

namespace QuantConnect.Tests.Brokerages.Kraken
{
    [TestFixture]
    public partial class KrakenBrokerageTests
    {
        private static TestCaseData[] TestParameters
        {
            get
            {
                return new[]
                {
                    new TestCaseData(Symbol.Create("EURUSD", SecurityType.Crypto, Market.Kraken), Resolution.Tick, 10000, false),
                    new TestCaseData(Symbol.Create("BTCUSD", SecurityType.Crypto, Market.Kraken), Resolution.Tick, 10000, false),
                    new TestCaseData(Symbol.Create("BTCUSD", SecurityType.Crypto, Market.Kraken), Resolution.Second, 10000, false),
                    new TestCaseData(Symbol.Create("BTCUSD", SecurityType.Crypto, Market.Kraken), Resolution.Minute, 60000, false),
                };
            }
        }

        [Test, TestCaseSource(nameof(TestParameters))]
        public void StreamsData(Symbol symbol, Resolution resolution, int waitMilliseconds, bool throwsException)
        {
            var cancelationToken = new CancellationTokenSource();
            var brokerage = (KrakenBrokerage)Brokerage;

            SubscriptionDataConfig[] configs;
            if (resolution == Resolution.Tick)
            {
                var tradeConfig = new SubscriptionDataConfig(GetSubscriptionDataConfig<Tick>(symbol, resolution), tickType: TickType.Trade);
                var quoteConfig = new SubscriptionDataConfig(GetSubscriptionDataConfig<Tick>(symbol, resolution), tickType: TickType.Quote);
                configs = new[] { tradeConfig, quoteConfig };
            }
            else
            {
                configs = new[] { GetSubscriptionDataConfig<QuoteBar>(symbol, resolution),
                    GetSubscriptionDataConfig<TradeBar>(symbol, resolution) };
            }

            foreach (var config in configs)
            {
                ProcessFeed(brokerage.Subscribe(config, (s, e) => { }),
                    cancelationToken,
                    (baseData) => { if (baseData != null) { Log.Trace($"{baseData}"); }
                    });
            }

            Thread.Sleep(waitMilliseconds);

            foreach (var config in configs)
            {
                brokerage.Unsubscribe(config);
            }

            Thread.Sleep(5000);

            cancelationToken.Cancel();
        }
    }
}