   M S B A M L   `   `   `  ����  
GameOfLife    GameOfLife.Window1   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Лог��5   	   $��694.65��6   $	��532��6%   .��5      � .+�5   
   &   System.Windows.Controls.DataGrid  -   5          Name$ logGrid��5      $
��Left=�6$   $	��142��6?   $��	35,51,0,0q�6L   $	��Top=�6_   $	��461��6w      AutoGenerateColumns .     EnableRowVirtualization .     CanUserAddRows .     SelectionMode$ Single=�6�      Columns 5      0  *System.Windows.Controls.DataGridTextColumn  5      ,  &System.Windows.Controls.DataGridColumn   Header$+ %Начальное состояние��6D      Width5  /System.Windows.Controls.DataGridLengthConverter$	 110 6a   	   
IsReadOnly	 . 1  +System.Windows.Controls.DataGridBoundColumn
   Binding
 6%   �� *
startstate+6m   4  .System.Windows.Controls.DataGridTemplateColumn  5	         CellTemplate 5
      �� 5      .R�5      �� -   5        .��5   A   Показать6K   5      5        5      $ Дата начала��6b   $	 110 6w   	 . 
 6%   �� *	startdate+ �� StringFormat$ {0:dd/MM/yyyy}��6�     5      $) #Конечное состояние��6B   $	 110 6^   	 . 
 6%   �� *
endstate+6j     5       5      �� 5      .R�5      �� -   5        .��5   ?   Показать6I   5      5        5      $! Дата окончания��6`   $	 110 6x   	 . 
 6%   �� *	enddate+$ {0:dd/MM/yyyy}��6�   5      (  "System.Windows.Controls.DatePicker  5   
   -   5          Name$ start��5      $
��Left=�6$   $��	97,10,0,0q�6?   $	��Top=�6R     5   
   -   5        $	 end��5      $
��Left=�6"   $��
325,10,0,0q�6=   $	��Top=�6Q   �� 5   
   -   5         ��Name$ generate��5      $��Сформировать��6#   $
��Left=�6:   $��24��6U   $��
204,10,0,0q�6a   $	��Top=�6u   $	��116��6�   6�   A� 5   
   -   5         A�Name$ 	BoardGrid��5   -    A� Columns$ 25��6    A� Rows$ 25��6#   ���	#FFCBCBCB$��60,222,64,43q�6W   5!      5#      5$      