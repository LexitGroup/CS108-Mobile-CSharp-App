﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

using System.Windows.Input;
using Xamarin.Forms;


using Plugin.BLE.Abstractions.Contracts;

using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Extensions;

namespace BLE.Client.ViewModels
{
    public class ViewModelReadWrite : BaseViewModel
    {
        private readonly IUserDialogs _userDialogs;

        public string entrySelectedEPC { get; set; }
        public string entrySelectedPWD { get; set; }
        public string entryPC { get; set; }
        public string entryEPC { get; set; }
        public string entryACCPWD { get; set; }
        public string entryKILLPWD { get; set; }
        public string entryTIDUID { get; set; }
        public string entryUSER { get; set; }

        public string labelPCStatus { get; set; } = "";
        public string labelEPCStatus { get; set; } = "";
        public string labelACCPWDStatus { get; set; } = "";
        public string labelKILLPWDStatus { get; set; } = "";
        public string labelTIDUIDStatus { get; set; } = "";
        public string labelUSERStatus { get; set; } = "";

        public Boolean switchPCIsToggled { get; set; } = true;
        public Boolean switchEPCIsToggled { get; set; } = true;
        public Boolean switchACCPWDIsToggled { get; set; } = true;
        public Boolean switchKILLPWDIsToggled { get; set; } = true;
        public Boolean switchTIDUIDIsToggled { get; set; } = true;
        public Boolean switchUSERIsToggled { get; set; } = false;

        UInt16 _labelTIDOffset = 0;
        UInt16 _labelTIDWord = 2;
        UInt16 _labelUSEROffset = 0;
        UInt16 _labelUSERWord = 2;
        public string labelTIDOffset { get { return "Offset=" + _labelTIDOffset.ToString(); } }
        public string labelTIDWord { get { return "Word=" + _labelTIDWord.ToString(); } }
        public string labelUSEROffset { get { return "Offset=" + _labelUSEROffset.ToString(); } }
        public string labelUSERWord { get { return "Word=" + _labelUSERWord.ToString(); } }

        public ICommand OnReadButtonCommand { protected set; get; }
        public ICommand OnWriteButtonCommand { protected set; get; }

        public ICommand OnLabelTIDOffsetTapped { protected set; get; }
        public ICommand OnLabelTIDWordTapped { protected set; get; }
        public ICommand OnLabelUSEROffsetTapped { protected set; get; }
        public ICommand OnLabelUSERWordTapped { protected set; get; }

        uint accessPwd;

        uint readPCRetryCnt = 0;
        uint readEPCRetryCnt = 0;
        uint readACCPWDRetryCnt = 0;
        uint readKILLPWDRetryCnt = 0;
        uint readTIDUIDRetryCnt = 0;
        uint readUSERRetryCnt = 0;
        uint writePCRetryCnt = 0;
        uint writeEPCRetryCnt = 0;
        uint writeACCPWDRetryCnt = 0;
        uint writeKILLPWDRetryCnt = 0;
        uint writeUSERRetryCnt = 0;

        public ViewModelReadWrite(IAdapter adapter, IUserDialogs userDialogs) : base(adapter)
        {
            _userDialogs = userDialogs;

            OnReadButtonCommand = new Command(OnReadButtonButtonClick);
            OnWriteButtonCommand = new Command(OnWriteButtonButtonClick);

            OnLabelTIDOffsetTapped = new Command(onlabelTIDOffsetTapped);
            OnLabelTIDWordTapped = new Command(onlabelTIDWordTapped);
            OnLabelUSEROffsetTapped = new Command(onlabelUSEROffsetTapped);
            OnLabelUSERWordTapped = new Command(onlabelUSERWOordTapped);
        }

        public override void Resume()
        {
            base.Resume();
            BleMvxApplication._reader.rfid.OnAccessCompleted += new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
        }

        public override void Suspend()
        {
            BleMvxApplication._reader.rfid.OnAccessCompleted -= new EventHandler<CSLibrary.Events.OnAccessCompletedEventArgs>(TagCompletedEvent);
            base.Suspend();
        }

        protected override void InitFromBundle(IMvxBundle parameters)
        {
            base.InitFromBundle(parameters);

            entrySelectedEPC = BleMvxApplication._SELECT_EPC;
            entrySelectedPWD = "00000000";
            entryPC = BleMvxApplication._SELECT_PC.ToString ("X4");
            entryEPC = BleMvxApplication._SELECT_EPC;

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryPC);
            RaisePropertyChanged(() => entryEPC);

            RaisePropertyChanged(() => switchPCIsToggled);
            RaisePropertyChanged(() => switchEPCIsToggled);
            RaisePropertyChanged(() => switchACCPWDIsToggled);
            RaisePropertyChanged(() => switchKILLPWDIsToggled);
            RaisePropertyChanged(() => switchTIDUIDIsToggled);
            RaisePropertyChanged(() => switchUSERIsToggled);

            RaisePropertyChanged(() => labelTIDOffset);
            RaisePropertyChanged(() => labelTIDWord);
            RaisePropertyChanged(() => labelUSEROffset);
            RaisePropertyChanged(() => labelUSERWord);
        }

        void TagCompletedEvent(object sender, CSLibrary.Events.OnAccessCompletedEventArgs e)
        {
            if (e.access == CSLibrary.Constants.TagAccess.READ)
			{
                switch (e.bank)
                {
                    case CSLibrary.Constants.Bank.PC:
                        if (e.success)
                        {
                            entryPC = BleMvxApplication._reader.rfid.Options.TagReadPC.pc.ToString();
                            RaisePropertyChanged(() => entryPC);

							labelPCStatus = "O";
						}
						else
						{
                            if (--readPCRetryCnt == 0)
                                labelPCStatus = "E";
                            else
                                ReadPC();
						}
						RaisePropertyChanged(() => labelPCStatus);
						break;

                    case CSLibrary.Constants.Bank.EPC:
                        if (e.success)
                        {
                            entryEPC = BleMvxApplication._reader.rfid.Options.TagReadEPC.epc.ToString();
                            RaisePropertyChanged(() => entryEPC);
							labelEPCStatus = "O";
						}
						else
						{
                            if (--readEPCRetryCnt == 0)
							    labelEPCStatus = "E";
                            else
                                ReadEPC();
						}
						RaisePropertyChanged(() => labelEPCStatus);
						break;

                    case CSLibrary.Constants.Bank.ACC_PWD:
                        if (e.success)
                        {
                            entryACCPWD = BleMvxApplication._reader.rfid.Options.TagReadAccPwd.password.ToString();
                            RaisePropertyChanged(() => entryACCPWD);
							labelACCPWDStatus = "O";
						}
						else
						{
                            if (--readACCPWDRetryCnt == 0)
                                labelACCPWDStatus = "E";
                            else
                                ReadACCPWD();
						}
						RaisePropertyChanged(() => labelACCPWDStatus);
						break;

                    case CSLibrary.Constants.Bank.KILL_PWD:
                        if (e.success)
                        {
                            entryKILLPWD = BleMvxApplication._reader.rfid.Options.TagReadKillPwd.password.ToString();
                            RaisePropertyChanged(() => entryKILLPWD);
							labelKILLPWDStatus = "O";
						}
						else
						{
                            if (--readKILLPWDRetryCnt == 0)
                                labelKILLPWDStatus = "E";
                            else
                                ReadKILLPWD();
						}
						RaisePropertyChanged(() => labelKILLPWDStatus);
						break;

                    case CSLibrary.Constants.Bank.TID:
                        if (e.success)
                        {
                            entryTIDUID = BleMvxApplication._reader.rfid.Options.TagReadTid.tid.ToString();
                            RaisePropertyChanged(() => entryTIDUID);
							labelTIDUIDStatus = "O";
						}
						else
						{
                            if (--readTIDUIDRetryCnt == 0)
                                labelTIDUIDStatus = "E";
                            else
                                ReadTIDUID();
						}
						RaisePropertyChanged(() => labelTIDUIDStatus);
						break;

                    case CSLibrary.Constants.Bank.USER:
                        if (e.success)
                        {
                            entryUSER = BleMvxApplication._reader.rfid.Options.TagReadUser.pData.ToString();
                            RaisePropertyChanged(() => entryUSER);
							labelUSERStatus = "O";
						}
						else
						{
                            if (--readUSERRetryCnt == 0)
                                labelUSERStatus = "E";
                            else
                                ReadUSER();
						}
						RaisePropertyChanged(() => labelUSERStatus);
						break;

                    case CSLibrary.Constants.Bank.SPECIFIC:
                        if (e.success)
                        {
                            entryKILLPWD = BleMvxApplication._reader.rfid.Options.TagRead.pData.ToString();
                            RaisePropertyChanged(() => entryKILLPWD);
                            labelKILLPWDStatus = "O";
                        }
                        else
                        {
                            if (--readKILLPWDRetryCnt == 0)
                                labelKILLPWDStatus = "E";
                            else
                                ReadKILLPWD();
                        }
                        RaisePropertyChanged(() => labelKILLPWDStatus);
                        break;

                }
            }

			if (e.access == CSLibrary.Constants.TagAccess.WRITE)
			{
				switch (e.bank)
				{
					case CSLibrary.Constants.Bank.PC:
						if (e.success)
						{
							labelPCStatus = "O";
						}
						else
						{
                            if (--writePCRetryCnt == 0)
                                labelPCStatus = "E";
                            else
                                WritePC ();
						}
						RaisePropertyChanged(() => labelPCStatus);
						break;

					case CSLibrary.Constants.Bank.EPC:
						if (e.success)
						{
							labelEPCStatus = "O";
						}
						else
						{
                            if (--writeEPCRetryCnt == 0)
                                labelEPCStatus = "E";
                            else
                                WriteEPC();
						}
						RaisePropertyChanged(() => labelEPCStatus);
						break;

					case CSLibrary.Constants.Bank.ACC_PWD:
						if (e.success)
						{
							labelACCPWDStatus = "O";
						}
						else
						{
                            if (--writeACCPWDRetryCnt == 0)
                                labelACCPWDStatus = "E";
                            else
                                WriteACCPWD();
						}
						RaisePropertyChanged(() => labelACCPWDStatus);
						break;

					case CSLibrary.Constants.Bank.KILL_PWD:
						if (e.success)
						{
							labelKILLPWDStatus = "O";
						}
						else
						{
                            if (--writeKILLPWDRetryCnt == 0)
                                labelKILLPWDStatus = "E";
                            else
                                WriteKILLPWD();
						}
						RaisePropertyChanged(() => labelKILLPWDStatus);
						break;

					case CSLibrary.Constants.Bank.USER:
						if (e.success)
						{
							labelUSERStatus = "O";
						}
						else
						{
                            if (--writeUSERRetryCnt == 0)
                                labelUSERStatus = "E";
                            else
                                WriteUSER();
						}
						RaisePropertyChanged(() => labelUSERStatus);
						break;

                    case CSLibrary.Constants.Bank.SPECIFIC:
                        if (e.success)
                        {
                            labelKILLPWDStatus = "O";
                        }
                        else
                        {
                            if (--writeKILLPWDRetryCnt == 0)
                                labelKILLPWDStatus = "E";
                            else
                                WriteKILLPWD();
                        }
                        RaisePropertyChanged(() => labelKILLPWDStatus);
                        break;


                }
            }
		}

        async void onlabelTIDOffsetTapped()
        {
            try
            {
                var msg = $"Enter a TID bank offset value (word)";
                this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
                var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _labelTIDOffset.ToString(), cancelToken: this.cancelSrc?.Token);
                await System.Threading.Tasks.Task.Delay(500);

                _labelTIDOffset = UInt16.Parse(r.Text);

            }
            catch (Exception ex)
            {
            }

            RaisePropertyChanged(() => labelTIDOffset);
        }

        async void onlabelTIDWordTapped()
        {
            try
            {
                var msg = $"Enter a TID bank length value (word)";
                this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
                var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _labelTIDWord.ToString(), cancelToken: this.cancelSrc?.Token);
                await System.Threading.Tasks.Task.Delay(500);

                _labelTIDWord = UInt16.Parse(r.Text);
            }
            catch (Exception ex)
            {
            }

            RaisePropertyChanged(() => labelTIDWord);
        }

        async void onlabelUSEROffsetTapped()
        {
            try
            {
                var msg = $"Enter a USER bank offset value (word)";
                this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
                var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _labelUSEROffset.ToString(), cancelToken: this.cancelSrc?.Token);
                await System.Threading.Tasks.Task.Delay(500);

                _labelUSEROffset = UInt16.Parse(r.Text);
            }
            catch (Exception ex)
            {
            }

            RaisePropertyChanged(() => labelUSEROffset);
        }

        async void onlabelUSERWOordTapped()
        {
            try
            {
                var msg = $"Enter a read length value (word)";
                this.cancelSrc?.CancelAfter(TimeSpan.FromSeconds(3));
                var r = await this._userDialogs.PromptAsync(msg, inputType: InputType.Number, placeholder: _labelUSERWord.ToString(), cancelToken: this.cancelSrc?.Token);
                await System.Threading.Tasks.Task.Delay(500);

                _labelUSERWord = UInt16.Parse(r.Text);
            }
            catch (Exception ex)
            {
            }

            RaisePropertyChanged(() => labelUSERWord);
        }

        System.Threading.CancellationTokenSource cancelSrc;

        void OnReadButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            uint m_retry_cnt = 7;       // Max 7

			labelPCStatus = "";
			labelEPCStatus = "";
			labelACCPWDStatus = "";
			labelKILLPWDStatus = "";
			labelTIDUIDStatus = "";
			labelUSERStatus = "";

			RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);

            RaisePropertyChanged(() => switchPCIsToggled);
            RaisePropertyChanged(() => switchEPCIsToggled);
            RaisePropertyChanged(() => switchACCPWDIsToggled);
            RaisePropertyChanged(() => switchKILLPWDIsToggled);
            RaisePropertyChanged(() => switchTIDUIDIsToggled);
            RaisePropertyChanged(() => switchUSERIsToggled);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            //BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);

            if (entrySelectedEPC.Length > 64)
            {
                //MessageBox.Show("EPC too long, only selecte first 256 bit");
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC.Substring(0, 64));
            }
            else
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            BleMvxApplication._reader.rfid.SetCurrentLinkProfile(BleMvxApplication._config.RFID_Profile);

            if (switchPCIsToggled)
            {
				labelPCStatus = "R";

                readPCRetryCnt = m_retry_cnt;
                ReadPC();
			}

			if (switchEPCIsToggled)
            {
				labelEPCStatus = "R";

                readEPCRetryCnt = m_retry_cnt;
                ReadEPC();
			}

			//if access bank is checked, read it.
			if (switchACCPWDIsToggled)
            {
				labelACCPWDStatus = "R";

                readACCPWDRetryCnt = m_retry_cnt;
                ReadACCPWD();
			}

			//if kill bank is checked, read it.
			if (switchKILLPWDIsToggled)
            {
				labelKILLPWDStatus = "R";

                readKILLPWDRetryCnt = m_retry_cnt;
                ReadKILLPWD();
            }

			//if TID-UID is checked, read it.
			if (switchTIDUIDIsToggled)
			{
				labelTIDUIDStatus = "R";

                readTIDUIDRetryCnt = m_retry_cnt;
                ReadTIDUID();
			}

			//if user bank is checked, read it.
			if (switchUSERIsToggled)
            {
				labelUSERStatus = "R";

                readUSERRetryCnt = m_retry_cnt;
                ReadUSER();
            }

			RaisePropertyChanged(() => labelPCStatus);
			RaisePropertyChanged(() => labelEPCStatus);
			RaisePropertyChanged(() => labelACCPWDStatus);
			RaisePropertyChanged(() => labelKILLPWDStatus);
			RaisePropertyChanged(() => labelTIDUIDStatus);
			RaisePropertyChanged(() => labelUSERStatus);
		}

		void OnWriteButtonButtonClick()
        {
            Xamarin.Forms.DependencyService.Get<ISystemSound>().SystemSound(1);

            RaisePropertyChanged(() => switchTIDUIDIsToggled);

            //if tid bank is checked, read it.
            if (switchTIDUIDIsToggled)
            {
                //_userDialogs.ShowError("TID only display, cannot write", 3000);

                ShowDialog("TID only display, cannot write");

                return;
            }

			uint m_retry_cnt = 7;       // Max 7

            RaisePropertyChanged(() => switchPCIsToggled);
            RaisePropertyChanged(() => switchEPCIsToggled);
            RaisePropertyChanged(() => switchACCPWDIsToggled);
            RaisePropertyChanged(() => switchKILLPWDIsToggled);
            RaisePropertyChanged(() => switchTIDUIDIsToggled);
            RaisePropertyChanged(() => switchUSERIsToggled);

            RaisePropertyChanged(() => entrySelectedEPC);
            RaisePropertyChanged(() => entrySelectedPWD);
            RaisePropertyChanged(() => entryPC);
            RaisePropertyChanged(() => entryEPC);
            RaisePropertyChanged(() => entryACCPWD);
            RaisePropertyChanged(() => entryKILLPWD);
            RaisePropertyChanged(() => entryTIDUID);
            RaisePropertyChanged(() => entryUSER);

            accessPwd = Convert.ToUInt32(entrySelectedPWD, 16);

            if (BleMvxApplication._reader.rfid.State != CSLibrary.Constants.RFState.IDLE)
            {
                //MessageBox.Show("Reader is busy now, please try later.");
                return;
            }

            // Can not write TID bank
            if (switchTIDUIDIsToggled)
            {
				return;
            }

            if (!(switchPCIsToggled | switchEPCIsToggled | switchACCPWDIsToggled | switchKILLPWDIsToggled | switchUSERIsToggled))
            {
                //All unchecked
                //MessageBox.Show("Please check at least one item to write", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
                return;
            }

            if (entrySelectedEPC.Length > 64)
            {
                //MessageBox.Show("EPC too long, only selecte first 256 bit");
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC.Substring(0, 64));
            }
            else
                BleMvxApplication._reader.rfid.Options.TagSelected.epcMask = new CSLibrary.Structures.S_MASK(/*m_record.pc.ToString() + */entrySelectedEPC);

            BleMvxApplication._reader.rfid.Options.TagSelected.flags = CSLibrary.Constants.SelectMaskFlags.ENABLE_TOGGLE;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskOffset = 0;
            BleMvxApplication._reader.rfid.Options.TagSelected.epcMaskLength = (uint)BleMvxApplication._reader.rfid.Options.TagSelected.epcMask.Length * 8;
            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_SELECTED);

            //if access bank is checked, read it.
            if (switchACCPWDIsToggled)
            {
				labelACCPWDStatus = "W";

                writeACCPWDRetryCnt = m_retry_cnt;
                WriteACCPWD();
            }

            //if kill bank is checked, read it.
            if (switchKILLPWDIsToggled)
            {
				labelKILLPWDStatus = "W";

                writeKILLPWDRetryCnt = m_retry_cnt;
                WriteKILLPWD();
            }

            //if user bank is checked, read it.
            if (switchUSERIsToggled)
            {
				labelUSERStatus = "W";

                writeUSERRetryCnt = m_retry_cnt;
                WriteUSER();
            }

            if (switchPCIsToggled)
            {
				labelPCStatus = "W";

                writePCRetryCnt = m_retry_cnt;
                WritePC();
            }
            
            //Write EPC must put in last order to prevent it get lost
            if (switchEPCIsToggled)
            {
				labelEPCStatus = "W";

                writeEPCRetryCnt = m_retry_cnt;
                WriteEPC();
            }

			RaisePropertyChanged(() => labelPCStatus);
			RaisePropertyChanged(() => labelEPCStatus);
			RaisePropertyChanged(() => labelACCPWDStatus);
			RaisePropertyChanged(() => labelKILLPWDStatus);
            RaisePropertyChanged(() => labelUSERStatus);
        }

        void ReadPC ()
        {
			BleMvxApplication._reader.rfid.Options.TagReadPC.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_PC);
		}

        void ReadEPC ()
        {
			BleMvxApplication._reader.rfid.Options.TagReadEPC.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagReadEPC.count = 6;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_EPC);
		}

        void ReadACCPWD ()
        {
			BleMvxApplication._reader.rfid.Options.TagReadAccPwd.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_ACC_PWD);
		}

        /* for testing
                void ReadKILLPWD ()
                {
                    BleMvxApplication._reader.rfid.Options.TagReadKillPwd.accessPassword = accessPwd;

                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_KILL_PWD);
                }
        */

        void ReadKILLPWD()
        {
            BleMvxApplication._reader.rfid.Options.TagRead.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.Options.TagRead.bank =  CSLibrary.Constants.MemoryBank.BANK1; // EPC Bank
            BleMvxApplication._reader.rfid.Options.TagRead.offset = 1; // 0 is CRC can not read
            BleMvxApplication._reader.rfid.Options.TagRead.count = 9;

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ);
        }


        void ReadTIDUID ()
		{
			BleMvxApplication._reader.rfid.Options.TagReadTid.accessPassword = accessPwd;
			BleMvxApplication._reader.rfid.Options.TagReadTid.offset = _labelTIDOffset;
			BleMvxApplication._reader.rfid.Options.TagReadTid.count = _labelTIDWord;

			BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_TID);
		}

        void ReadUSER ()
        {
			BleMvxApplication._reader.rfid.Options.TagReadUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagReadUser.offset = _labelUSEROffset; // m_readAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagReadUser.count = _labelUSERWord; // m_readAllBank.WordUser;

			BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_READ_USER);
        }

        void WriteACCPWD ()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteAccPwd.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteAccPwd.password = Convert.ToUInt32(entryACCPWD, 16);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_ACC_PWD);
        }

        /*
                void WriteKILLPWD ()
                {
                    BleMvxApplication._reader.rfid.Options.TagWriteKillPwd.accessPassword = accessPwd;
                    BleMvxApplication._reader.rfid.Options.TagWriteKillPwd.password = Convert.ToUInt32(entryKILLPWD, 16);

                    BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_KILL_PWD);
                }
        */

        void WriteKILLPWD()
        {
            BleMvxApplication._reader.rfid.Options.TagWrite.accessPassword = accessPwd;

            BleMvxApplication._reader.rfid.Options.TagWrite.bank = CSLibrary.Constants.MemoryBank.BANK1; // EPC Bank
            BleMvxApplication._reader.rfid.Options.TagWrite.offset = 1; // 0 is CRC can not read
            BleMvxApplication._reader.rfid.Options.TagWrite.count = 9;
            BleMvxApplication._reader.rfid.Options.TagWrite.pData = CSLibrary.Tools.Hex.ToUshorts(entryKILLPWD);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE);
        }


        void WriteUSER ()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteUser.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.offset = _labelUSEROffset; // m_writeAllBank.OffsetUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.count = _labelUSERWord; // m_writeAllBank.WordUser;
            BleMvxApplication._reader.rfid.Options.TagWriteUser.pData = CSLibrary.Tools.Hex.ToUshorts(entryUSER);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_USER);
        }

        void WritePC ()
        {
            BleMvxApplication._reader.rfid.Options.TagWritePC.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWritePC.pc = CSLibrary.Tools.Hex.ToUshort(entryPC);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_PC);
        }
            
        void WriteEPC ()
        {
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.accessPassword = accessPwd;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.offset = 0;
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.count = CSLibrary.Tools.Hex.GetWordCount(entryEPC);
            BleMvxApplication._reader.rfid.Options.TagWriteEPC.epc = new CSLibrary.Structures.S_EPC(entryEPC);

            BleMvxApplication._reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_WRITE_EPC);
        }

        async void ShowDialog(string Msg)
        {
            var config = new ProgressDialogConfig()
            {
                Title = Msg,
                IsDeterministic = true,
                MaskType = MaskType.Gradient,
            };

            using (var progress = _userDialogs.Progress(config))
            {
                progress.Show();
                await System.Threading.Tasks.Task.Delay(3000);
            }
        }

    }
}