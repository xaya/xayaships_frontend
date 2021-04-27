﻿// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

namespace BitcoinLib.RPC.Specifications
{
    //  Note: Do not alter the capitalization of the enum members as they are being cast as-is to the RPC server
    public enum RpcMethods
    {
        //== Blockchain ==
        getbestblockhash,
        getblock,
        getblockchaininfo,
        getblockcount,
        getblockhash,
        getblockheader,
        getchaintips,
        getdifficulty,
        getmempoolinfo,
        getrawmempool,
        gettxout,
        gettxoutproof,
        gettxoutsetinfo,
        verifychain,
        verifytxoutproof,

        //== Control ==
        getinfo,
        help,
        stop,

        //== Generating ==
        generate,
        getgenerate,
        setgenerate,

        //== Mining ==
        getblocktemplate,
        getmininginfo,
        getnetworkhashps,
        prioritisetransaction,
        submitblock,

        //== Network ==
        addnode,
        clearbanned,
        disconnectnode,
        getaddednodeinfo,
        getconnectioncount,
        getnettotals,
        getnetworkinfo,
        getpeerinfo,
        listbanned,
        ping,
        setban,

        //== Rawtransactions ==
        createrawtransaction,
        decoderawtransaction,
        decodescript,
        fundrawtransaction,
        getrawtransaction,
        sendrawtransaction,
        signrawtransaction,
        sighashtype,

        //== Util ==
        createmultisig,
        estimatefee,
        estimatepriority,
        estimatesmartfee,
        estimatesmartpriority,
        validateaddress,
        mirroraddress,
        verifymessage,

        //== Wallet ==
        abandontransaction,
        addmultisigaddress,
        addwitnessaddress,
        backupwallet,
        dumpprivkey,
        dumpwallet,
        getaccount,
        getaccountaddress,
        getaddressesbyaccount,
        getbalance,
        getnewaddress,
        getrawchangeaddress,
        getreceivedbyaccount,
        getreceivedbyaddress,
        gettransaction,
        getunconfirmedbalance,
        getwalletinfo,
        importaddress,
        importprivkey,
        importpubkey,
        importwallet,
        keypoolrefill,
        listaccounts,
        listaddressgroupings,
        listlockunspent,
        listreceivedbyaccount,
        listreceivedbyaddress,
        listsinceblock,
        listtransactions,
        listmirrtransactions,
        listunspent,
        lockunspent,
        move,
        sendfrom,
        sendmany,
        sendtoaddress,
        setaccount,
        settxfee,
        signmessage,
        walletlock,
        walletpassphrase,
        walletpassphrasechange,

        //XAYA
        name_update,
        name_list,
        name_register,
        name_show,
        name_pending,
        name_scan,
        sendtoname,
        name_history,
        waitforchange,
        getcurrentstate
    }
}