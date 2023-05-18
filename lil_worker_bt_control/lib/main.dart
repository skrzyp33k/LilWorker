import 'package:flutter/material.dart';
import 'package:lil_worker_bt_control/bluetooth_off_screen.dart';
import 'package:lil_worker_bt_control/find_devices_screen.dart';
import 'package:lil_worker_bt_control/palette.dart';
import 'package:flutter_blue_plus/flutter_blue_plus.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'LilWorker WiFi connect',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          color: Colors.black,
          titleTextStyle: TextStyle(
            fontSize: 22.0,
            fontWeight: FontWeight.bold,
          )
        ),
        useMaterial3: true,
        colorScheme: const ColorScheme.dark().copyWith(
          primary: GbPalette.yellow,

        ),
      ),
      //home: const MyHomePage(title: 'LilWorker WiFi connect'),
      home: StreamBuilder<BluetoothState>(
        stream: FlutterBluePlus.instance.state,
        initialData: BluetoothState.unknown,
        builder: (c, snapshot){
          final state = snapshot.data;
          if(state == BluetoothState.on)
            {
              return const FindDevicesScreen();
            }
          return BluetoothOffScreen(state: state);
        }
      ),
    );
  }
}
