import 'dart:async';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

import 'package:lilworker_app/palette.dart';

void main() => runApp(const MyApp());

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: 'LilWorker App',
      theme: ThemeData(
        primarySwatch: GbPalette.yellow,
      ),
      home: const MyHomePage(),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key});

  @override
  MyHomePageState createState() => MyHomePageState();
}

class MyHomePageState extends State<MyHomePage> {
  String _ssid = '';
  String _password = '';
  bool _isLoading = false;

  Future<bool> findLilWorker() async {
    final socket = await RawDatagramSocket.bind(InternetAddress.anyIPv4, 0);
    final data = utf8.encode('LilWorker?');
    socket.send(data, InternetAddress('192.168.1.1'), 60000);

    final completer = Completer<bool>();

    socket.listen((event) {
      final datagram = socket.receive();
      if (datagram != null) {
        final message = utf8.decode(datagram.data);
        if (message.startsWith('LilWorker-')) {
          completer.complete(true);
          socket.close();
        } else {
          completer.complete(false);
          socket.close();
        }
      }
    });

    return completer.future.timeout(const Duration(seconds: 5), onTimeout: () {
      socket.close();
      return false;
    });
  }


  Future<void> saveCredentials() async {
    final url = Uri.parse('http://192.168.1.1/save');
    final response = await http.post(
      url,
      body: {'ssid': _ssid, 'password': _password},
    );
    if (response.statusCode == 200) {
      Navigator.pop(context);
    }
  }

  void checkForLilWorker() async {
    setState(() {
      _isLoading = true;
    });
    bool founded = await findLilWorker();
    setState(() {
      _isLoading = false;
    });
    if (founded) {
      showDialog(
        context: context,
        builder: (BuildContext context) {
          return AlertDialog(
            title: const Text('LilWorker found!'),
            contentPadding: const EdgeInsets.all(16),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  onChanged: (value) {
                    setState(() {
                      _ssid = value;
                    });
                  },
                  decoration: const InputDecoration(labelText: 'SSID'),
                ),
                TextField(
                  onChanged: (value) {
                    setState(() {
                      _password = value;
                    });
                  },
                  decoration: const InputDecoration(labelText: 'Password'),
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () {
                  saveCredentials();
                },
                child: const Text('Save'),
              ),
            ],
          );
        },
      );
    } else {
      showDialog(
        context: context,
        builder: (BuildContext context) {
          return AlertDialog(
            title: const Text('LilWorker not found!'),
            contentPadding: const EdgeInsets.all(16),
            content: const Text('LilWorker not found in this network.'),
            actions: [
              TextButton(
                onPressed: () {
                  Navigator.pop(context);
                },
                child: const Text('Ok'),
              ),
            ],
          );
        },
      );

    }
  }
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('LilWorker App'),
        centerTitle: true,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            ElevatedButton(
              onPressed: _isLoading ? null : checkForLilWorker,
              child: const Text('Check if LilWorker exists in this network'),
            ),
            const SizedBox(height: 16),
            if (_isLoading)
              const CircularProgressIndicator(),
          ],
        ),
      ),
    );
  }
}