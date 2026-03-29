
Before play the Game you need:

1. Import DOTween plugin
https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676

2. Install iOS 14 Advertising Support package
https://docs.unity3d.com/Packages/com.unity.ads.ios-support@1.0/manual/index.html

3. Setup IAP
https://www.notion.so/Set-up-IAP-1e4ce6b4237c8024a0c3d875066aec0a

4. Setup AdMob
https://www.notion.so/Set-up-AdMob-18bce6b4237c801f8774e7cc47a55daf



Please read "EXPLANATORY_DOC" in Documentation folder before and with notes below.

Key notes:
0. Reception, Elevator, Utility are mandatory for every Hotel. In a single instance each.
1. Every Hotel consists of Areas.

2. Area numbers should start with number one(1).
3. Area number one(1) is enabled by default. Areas number two(2), three(3), four(4) etc. needs to be purchased while play.

4. Room numbers should start with number zero(0).
5. Room number zero(0) is enabled by default. Rooms number two(2), three(3), four(4) etc. needs to be purchased while play.

6. After Customer(Unit) left Room, system will check if there is available ToiletCabine. If YES - Customer will go there. If NOT - leave the Hotel.
7. System will check only for ToiletCabine with the same Area number that has Room, Customer was in.

8. Cleaner cleans only Rooms with which they has the same Area number.

9. Every Hotel has Level. Level by default is one(1).
10. Every Hotel has Progress value and MaxProgress. Progress by default is zero(0).
11. MaxProgress value for each Level are getting from Rooms and Toilets parameter "PurchaseProgressReward" and "UpdateProgressReward", that belong to Area with Lvl parameter equal to Level.

12. By purchasing(upgrading) Room or Toilet, paremeter "PurchaseProgressReward" or "UpdateProgressReward" values will be added to the Progress.
13. Once Progress equal or more then MaxProgress, Level encreased by 1.
14. When Level changed, Area which can be purchased at that Level is becoming available to purchase.

15. For Room number zero(0), parameter "PurchaseProgressReward" must be zero(0).
Since this Room is available by default and this value can never be added to the Progress value.

16. Elevator will swith to "ElevatorNoHotelSceneState" state if there is no next Hotel in Build Settins.
For example: your current Hotel is one(scene index = 1 in Build Settings) and you dont have scene with index = 2.

17. Loader delivers ToiletPaper to the Toilet whatever Area Toilet is.

18. To increase/decrease transaction speed modify _amountDefault parameter in the ItemController script.
19. Set _stateManager.IsSendLogs = false to turn off the Logs for particular StateManager. See CashController.cs for example.



Developer(cheat) Panel.
1. Developer panel button could be found in the Settings and only visible when Debug build.



ADS.
1. INT ads will be triggered to show on each Entity purchase/update.
2. REW ads could be triggered to show for test purposes from the Developer panel.



There are detailed tutorials of how to:
1. Add New Hotel. Part 1. Create Entities
https://youtu.be/Vm-xTl3Q2Fw

2. Add New Hotel. Part 2. Add Entities
https://youtu.be/sQ_woJMW9Pc

3. Add new HUD
https://youtu.be/Ea70idS4HE4

4. Replace Unit (Cleaner example)
https://youtu.be/lQUWYV5QHTY

5. Replace Player
https://www.notion.so/Replace-Player-2e3ce6b4237c8047a48dc09740588b40



Game has GameConfig(Resources/GameConfig).
1. "CashPileRadius", "ReceptionItemRadius" and other radiuses - distance to the correspondent items, Player can start interact with.
2. "ToiletPaperFlyTime" - time toilet paper flies from Player to the ToiletCabine.
3. "ToiletVisitsCountMax" - number of visits to the ToiletCabine before it needs to be refilled with toilet paper.
4. "InventoryMax" - maximum amount of inventory Player can carry.
5. AdsProviderEditor - provider from which ads will be served on Editor.
6. AdsProvider - provider from which ads will be served on Build.



Contact me if you have any question:
aleksandr.gorodiski@gmail.com

P.S. please send the purchase invoice number in your email