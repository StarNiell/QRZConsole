[ENG]
************************************************************************************************************************
Welcome to QRZ Console v.1.0.0.8-beta

QRZ Console is a "unofficial command-line interface" to the QRZ.COM website and require a personal account.
For more information about the website, go to: https://www.qrz.com

This program require Microsoft Windows EDGE Browser (pre-installed since Windows 8.1)
************************************************************************************************************************

[ITA]
************************************************************************************************************************
La presente release è una beta! Non si considera quindi "esente da bug".
Il suo funzionamento, basato sull'automazione di un browser web, è ancora in fase di ottimizzazione

Al primo avvio del software si consiglia di visualizzare l'elenco dei comandi disponibili, digitando "cm" INVIO
(troverete l'eneco dei comandi disponibili più in basso in questa pagina).

Come potete notare, i comandi sono composti da due caratteri e, nei limiti del possibile, sono rappresentazioni
mnemoniche delle funzioni che svolgono.

Alcuni comandi eseguono solo un'azione, altri necessitano di uno o più parametri, altri ancora restituiscono dati.
I comandi eseguiti finiscono nella history (per singola sessione) e per recuperarli è sufficiente premere 
Freccia SU quando il cursore è presente nella casella dei comandi. E' possibile riportare il cursore nella 
casella dei comandi premendo il tasto F2. Con il tasto Freccia GIU è possibile scorre l'history in avanti.

ATTENZIONE! Quando usate il comando Delete QSO ("dq"): prima di eseguirlo assicuratevi che la posizione da cancellare
corrisponda a quella selezionata. La posizione di un QSO è determinata dall'ordinamento del Logbook. Utilizzare
i comandi "da" e "dd" per stabilire l'ordine dei QSO nel Logbook. (per ragioni di sicurezza, il comand "dq" richiede
l'inserimento di 3 parametri, tutti uguali, corrispondenti alla posizione da cancellare.

In fase di aggiunta di un QSO al Logbook con l'utilizzo del comand "aq" il sistema aggiunge automaticamente, 
dopo il comando "aq" SPAZIO ... l'ultimo QRZ in memoria, l'ultima frequenza utilizzata e l'ultimo modo (SSB, AM, FM, ecc...)
nonchè la data e l'ora attuale in formato UTC. E' possibile spostarsi tra i vari parametri usandi il tasto TAB per scorrere 
verso destra oppure SHIFT+TAB per scorrere verso sinistra.

Date un'occhiata all'elenco di tutti i comandi prima di inziare.

Buona Radio e ottimi QSO!

IU8NQI
aniello.dinardo@gmail.com
************************************************************************************************************************

Command List
-------------------------------------------------------------------------
  Description                           command [param] ...
-------------------------------------------------------------------------
Credentials:
  Is Logged                             il
  Is Logged (last status)               ls
  Login                                 li [username] [password]
  LogOut                                lo
Search:
  Lookup                                lu [qrz]
  Check Worked                          cw [qrz]
  Lookup and Check Worked               lw [qrz]
  Check Worked and Lookup               wl [qrz]
Logbook:
  Add QSO                               aq [qrz] [freq] [mode] [date] [time] [comment]
  Edit QSO                              eq [position] [qrz] [freq] [mode] [date] [time] [comment]
  Delete QSO                            dq [position] [position] [position] (enter 3 times the same position to delete for security reasons)
  Open Logbook                          lb
  QSO Count                             qc
  Logbook pages                         lp
  Get/Set QSO for page                  qp [entries] (valid values: 5, 10, 15, 20, 25, 50, 100, 200)
  Get/Set current view mode             cv [mode] (mode: [raw], [text] OR [adif])
  Goto Page                             gp [page]
  Page Down                             pd
  Page Up                               pu
  Current Page                          cp
  Order Date Asc                        da
  Order Date Desc                       dd
  Get Table Contente Text               tt [page]
  Get Table Contente Raw                tr [page]
  Get Table Content XML                 tx [page]
  Get QSOs by range Text View           qt [start position] [end position]
  Get QSOs by range Text Raw            qr [start position] [end position]
  Get QSOs by range ADIF                qa [start position] [end position]
  Get QSOs by range ADIF                qa [start position] [end position]
Cluster DX:
  Open Cluster DX                       dx
  Show DX on Band                       sb [band] [items] (band example: [40m] [10m] or [hf] [vhf] [uhf])
  Show DX on Freq                       sf [freq] [items] (freq example: [7155] [14200] ...)
  Search Callsign on Cluster DX         ds [callsign]
  DXSpider command                      dc [DXSpider command: http://www.dxcluster.org/main/usermanual_en-12.html]
  Send Spot                             ss [call] [freq] [comment]
  Close Cluster DX                      cc
General/Utility:
  Clear Monitor                         cl
  Find DXCC Countries                   fc [search]
  Switch View                           sw
  Switch Check Is Logged at startup     sc
  Switch screen (normal/fullsize)       fs
  Last data in memory                   ld
  Command List                          cm
  Shortcut List                         sl
  Quit                                  qi
-------------------------------------------------------------------------


>dx
Clusterd DX connected: gb7ujs.ham-radio-op.net:7373
Login Cluster DX...
Cluster DX Logged ln

>sb hf
  21380.0 KG6UO        8-Apr-2022 2149Z USB                          <KF0ENS>
  14080.0 WV8DOH       8-Apr-2022 2149Z ft4 tnx                       <EA3CG>
  14317.0 VE6FXL       8-Apr-2022 2149Z                               <N5MKY>
  18100.0 ST2NH        8-Apr-2022 2148Z FT8                          <JH0BQX>
  14242.0 59           8-Apr-2022 2148Z OHIO                         <KB8ZXI>
  28496.0 VE2CSI       8-Apr-2022 2148Z 73 Steve                      <CX7FH>
  14075.5 ZX4X         8-Apr-2022 2147Z                              <LU1EEP>
  28075.7 LU9HZM       8-Apr-2022 2147Z FT8  Sent: -04  Rcvd: -02    <LU4DRH>
  18100.0 HD8MD        8-Apr-2022 2147Z FT8                            <HK3T>
  14080.0 KW4J         8-Apr-2022 2147Z FT4 -12dB from EM63 962Hz    <DL1NCH>
  18082.5 AB1WX        8-Apr-2022 2146Z MA                           <JR1NHD>
  21074.0 N2YCH        8-Apr-2022 2146Z TNX QSO                      <LU7DUE>
  21074.0 7Q7CT        8-Apr-2022 2146Z                              <JR3VXR>
  28440.0 OA4DVC       8-Apr-2022 2146Z                               <K9CCW>
  14076.5 ZF2CH        8-Apr-2022 2146Z tnx new one!!!               <IZ3ERM>
  14293.0 S57DX        8-Apr-2022 2146Z good copy in ohio            <KB8ZXI>
  28074.8 W0OCD        8-Apr-2022 2146Z FT8  Sent: -06  Rcvd: +00 EN3<LW2EDM>
  14014.0 G4DKM        8-Apr-2022 2145Z CQ gud sigs                   <G4GTW>
  21140.0 V31MA        8-Apr-2022 2145Z ft4 cq -05db,.               <JF1KKV>
  28496.0 VE2CSI       8-Apr-2022 2145Z Tnx QSO, 73 USB              <PU3YST>
  28444.0 PY7BR        8-Apr-2022 2144Z Good Sig tu                  <VE6KDX>
  14237.0 IU3BTY       8-Apr-2022 2144Z                               <N5MKY>
  28075.7 KC7QY        8-Apr-2022 2144Z FT8  Sent: -18  Rcvd: -09    <LU4DRH>
  14074.0 N0DOW        8-Apr-2022 2144Z FT8 tnx                       <EA3CG>
  14080.0 WV8DOH       8-Apr-2022 2144Z FT4 -08dB 2400Hz             <DL1NCH>
  28444.0 PY7BR        8-Apr-2022 2144Z CQ CQ WWFF                    <PY2NF>
  14074.6 OH6HOW       8-Apr-2022 2143Z FT8                          <LU1EEP>
  14214.0 EH4WRD       8-Apr-2022 2143Z                               <N5MKY>
  28074.8 K5BZI        8-Apr-2022 2143Z FT8  Sent: -05  Rcvd: -12 EM1<LW2EDM>
  18155.0 GI0UTE       8-Apr-2022 2142Z 55 NC great talking to you De<WX4DAT>
