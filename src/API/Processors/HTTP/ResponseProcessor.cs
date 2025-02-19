using API;
using API.Handlers;
using API.Models;
using Shared;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Processors.HTTP;
internal static class ResponseProcessor
{
    private static JsonWriterOptions _writerOptions => new JsonWriterOptions()
    {
        Indented = true
    };

    internal static void ProcessRequest(ref Span<byte> requestContext, Stream response, SQLiteConnection sqLiteConnection)
    {
        response.Write("\"result\":"u8);
        var method = RequestSerializer.GetValueAs<string>(ref requestContext, "method");
        if (method == null) return;
        switch (method.ToLower())
        {
            case "eth_currencysymbol":
                response.Write("\"ETH\""u8);
                break;

            case "net_version":
                response.Write(Setting.NetWorkIdFormattedByte);
                break;

            case "eth_chainid":
                response.Write(Setting.ChainIdFormattedByte);
                break;

            case "eth_getcode":
                var codeDetails = RequestSerializer.GetArrayAs<string>(ref requestContext, "params", 2);
                response.Write(RequestHandler.ProcessEthGetCode(codeDetails[0], codeDetails[1]));
                break;

            case "eth_gasprice":
                response.Write(Setting.GasPriceFormattedByte);
                break;

            case "eth_estimategas":
                var estimateGas = RequestSerializer.GetArrayAs<EstimateGas>(ref requestContext, "params", 1);
                response.Write(RequestHandler.ProcessEthEstimateGas(ref estimateGas[0]));
                break;
            case "eth_gettransactioncount":
                var transectionDetails = RequestSerializer.GetArrayAs<string>(ref requestContext, "params", 2);
                response.Write(RequestHandler.ProcessEthGetTransactionCount(transectionDetails[0], transectionDetails[1], sqLiteConnection));
                break;
            case "eth_sendrawtransaction":
                var transactionDetailsRange = RequestSerializer.GetArrayAs<Range>(ref requestContext, "params", 1);
                var transactionDetails = requestContext[transactionDetailsRange[0]];
                response.Write(RequestHandler.ProcessEthSendTransaction(ref transactionDetails, sqLiteConnection));
                break;
            case "eth_getblockbynumber":
            case "eth_getblockbyhash":
                var blockIdentifier = RequestSerializer.GetValueFromArray<string>(ref requestContext, "params", 0);
                var fullData = RequestSerializer.GetValueFromArray<bool>(ref requestContext, "params", 1);
                var writer = new Utf8JsonWriter(response, _writerOptions);
                RequestHandler.ProcessEthGetBlockByNumber(blockIdentifier,
                    fullData,
                    method.Equals("eth_getblockbyhash", StringComparison.CurrentCultureIgnoreCase), 
                    sqLiteConnection, 
                    writer);
                writer.Dispose();
                break;

            case "bb_getaddress":
            case "bb_getbalancehistory":
            case "bb_getblockhash":
            case "bb_gettickerslist":
            case "bb_gettx":
            case "bb_gettxspecific":
            case "bn_gasprice":
            case "cg_simpleprice":
            case "dc_gettokenholders":
            case "debug_gcstats":
            case "debug_getbadblocks":
            case "debug_getblockreceipts":
            case "debug_getblockreciepts":
            case "debug_getrawreceipts":
            case "debug_gettrieflushinterval":
            case "debug_storagerangeat":
            case "debug_traceblock":
            case "debug_traceblockbyhash":
            case "debug_traceblockbynumber":
            case "debug_tracecall":
            case "debug_tracecallmany":
            case "debug_tracetransaction":
            case "eth_accounts":
            case "eth_blobbasefee":
            case "eth_blocknumber":
            case "eth_call":
            case "eth_callmany":
            case "eth_cancelprivatetransaction":
            case "eth_coinbase":
            case "eth_createaccesslist":
            case "eth_feehistory":
            case "eth_getaccount":
            case "eth_getbalance":
            case "eth_getblockreceipts":
            case "eth_getblocktransactioncountbyhash":
            case "eth_getblocktransactioncountbynumber":
            case "eth_getfilterchanges":
            case "eth_getfilterlogs":
            case "eth_getheaderbynumber":
            case "eth_getlogs":
            case "eth_getproof":
            case "eth_getrawtransactionbyhash":
            case "eth_getstorageat":
            case "eth_gettransactionbyblockhashandindex":
            case "eth_gettransactionbyblocknumberandindex":
            case "eth_gettransactionbyhash":
            case "eth_gettransactionreceipt":
            case "eth_getunclebyblockhashandindex":
            case "eth_getunclebyblocknumberandindex":
            case "eth_getunclecountbyblockhash":
            case "eth_getunclecountbyblocknumber":
            case "eth_getuseroperationreceipt":
            case "eth_getwork":
            case "eth_hashrate":
            case "eth_maxpriorityfeepergas":
            case "eth_mining":
            case "eth_newblockfilter":
            case "eth_newfilter":
            case "eth_newpendingtransactionfilter":
            case "eth_pendingtransactions":
            case "eth_protocolversion":
            case "eth_sendprivatetransaction":
            case "eth_sendtransaction":
            case "eth_sign":
            case "eth_signtransaction":
            case "eth_submithashrate":
            case "eth_submitwork":
            case "eth_subscribe":
            case "eth_syncing":
            case "eth_uninstallfilter":
            case "eth_unsubscribe":
            case "gp_maliciouscheck":
            case "gp_tokensecurity":
            case "mt_addresslabel":
            case "mt_addressriskscore":
            case "net_listening":
            case "net_peercount":
            case "ots_getapilevel":
            case "ots_getblockdetails":
            case "ots_getblockdetailsbyhash":
            case "ots_getblocktransactions":
            case "ots_getcontractcreator":
            case "ots_getinternaloperations":
            case "ots_gettransactionbysenderandnonce":
            case "ots_gettransactionerror":
            case "ots_hascode":
            case "ots_searchtransactionsafter":
            case "ots_searchtransactionsbefore":
            case "ots_tracetransaction":
            case "parity_getblockreceipts":
            case "parity_pendingtransactions":
            case "qn_broadcastrawtransaction":
            case "view on Marketplace":
            case "qn_fetchaddressesbytag":
            case "qn_fetchnftcollectiondetails":
            case "qn_fetchnfts":
            case "qn_fetchnftsbycollection":
            case "qn_fetchnftsbycreator":
            case "qn_getblockwithreceipts":
            case "qn_getreceipts":
            case "qn_getsupportedchains":
            case "qn_gettokenmetadatabycontractaddress":
            case "qn_gettokenmetadatabysymbol":
            case "qn_gettransactionreceiptsbyaddress":
            case "qn_gettransactionsbyaddress":
            case "qn_gettransfersbynft":
            case "qn_getwallettokenbalance":
            case "qn_getwallettokentransactions":
            case "qn_verifynftsowner":
            case "rpc_modules":
            case "tb_getappearances":
            case "trace_block":
            case "trace_call":
            case "trace_callmany":
            case "trace_filter":
            case "trace_get":
            case "trace_rawtransaction":
            case "trace_replayblocktransactions":
            case "trace_replaytransaction":
            case "trace_transaction":
            case "txpool_content":
            case "txpool_contentfrom":
            case "txpool_inspect":
            case "txpool_status":
            case "v1/getcurrentexchangerates":
            case "web3_clientversion":
            case "web3_sha3":
            default:
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(method);
                Console.ResetColor();
                Console.WriteLine();
                break;
        }
    }

    internal static void SetUpHeaders(HttpListenerContext context)
    {
        context.Response.Headers.Clear();
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "POST");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "content-type");
        context.Response.Headers.Add("content-type", "application/json");

    }
}
