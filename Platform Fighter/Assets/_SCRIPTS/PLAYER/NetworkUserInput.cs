using System;
using System.Collections.Generic;
using System.Linq;
using MANAGERS;
using MISC;
using NETWORKING;
using Rewired;
using UnityEngine;
using Types = DATA.Types;

namespace PLAYER
{
    public class NetworkUserInput : InputSender
    {
        public int Id { get; set; }

        private List<P2PInputSet.InputChange> _changedInputs;

        [SerializeField]
        private List<P2PInputSet> _delayedInputSets;
        
        private P2PInputSet _lastInputSet;
        
        private Player _player;
        
        private int _jumpFramesHeld;
        
        private void Start()
        {
            base.Awake();
            _changedInputs = new List<P2PInputSet.InputChange>();
            _delayedInputSets = new List<P2PInputSet>();
            _player = ReInput.players.GetPlayer(Id);
            
            _player.controllers.maps.SetMapsEnabled(false, "Menu");
            _player.controllers.maps.SetMapsEnabled(true, "Default");
        }

        protected override void PressEvent(int index)
        {
            _changedInputs.Add(new P2PInputSet.InputChange((Types.Input) index, RealTimeInputs[index]));
        }

        protected override void ReleaseEvent(int index)
        {
            _changedInputs.Add(new P2PInputSet.InputChange((Types.Input) index, RealTimeInputs[index], InputFramesHeld[index]));
        }
        
        protected override void InputUpdate()
        {
            UpdatePlayerInput();
        }

        private void ApplyDelayedInputSets()
        {
            foreach (var input in _delayedInputSets.First().Inputs)
            {
                Inputs[(int) input.InputType] = input.State;
            }
            _delayedInputSets.RemoveAt(0);
            
            if(Inputs[(int) Types.Input.Jump])
            {
                if(_jumpFramesHeld == 0 && GetComponent<PlayerFlags>().GetFlagState(Types.Flags.ShortHop) != Types.FlagState.Pending)
                    GetComponent<PlayerFlags>().SetFlagState(Types.Flags.FullHop, Types.FlagState.Pending);
                
                if (_jumpFramesHeld < 7 && GetComponent<PlayerFlags>().GetFlagState(Types.Flags.FullHop) != Types.FlagState.Pending)
                {
                    GetComponent<PlayerFlags>().SetFlagState(Types.Flags.FullHop, Types.FlagState.Resolved);
                    GetComponent<PlayerFlags>().SetFlagState(Types.Flags.ShortHop, Types.FlagState.Pending);
                }
 
                ++_jumpFramesHeld;
            }
            else
                _jumpFramesHeld = 0;
        }
        
        private void UpdatePlayerInput()
        {
            for (var index = 0; index < RealTimeInputs.Length; index++)
            {
               RealTimeInputs[index] = false;
            }
            
            if (_player.controllers.hasKeyboard)
            {
                if (_player.GetAxis("Run") < -GameSettings.Instance.runThreshold)
                    RealTimeInputs[(int) Types.Input.StrongLeft] = true;
                else if (_player.GetAxis("Move") < 0)
                    RealTimeInputs[(int) Types.Input.LightLeft] = true;

                if (_player.GetAxis("Run") > GameSettings.Instance.runThreshold)
                    RealTimeInputs[(int) Types.Input.StrongRight] = true;
                else if (_player.GetAxis("Move") > 0)
                    RealTimeInputs[(int) Types.Input.LightRight] = true;

                if (_player.GetAxis("Crouch") < GameSettings.Instance.crouchThreshold)
                    RealTimeInputs[(int) Types.Input.Down] = true;
                else if (_player.GetAxis("Crouch") > GameSettings.Instance.upThreshold)
                    RealTimeInputs[(int) Types.Input.Up] = true;
            }
            else
            {
                if (_player.GetAxis("Move") < -GameSettings.Instance.runThreshold)
                    RealTimeInputs[(int) Types.Input.StrongLeft] = true;
                else if (_player.GetAxis("Move") < 0)
                    RealTimeInputs[(int) Types.Input.LightLeft] = true;

                if (_player.GetAxis("Move") > GameSettings.Instance.runThreshold)
                    RealTimeInputs[(int) Types.Input.StrongRight] = true;
                else if (_player.GetAxis("Move") > 0)
                    RealTimeInputs[(int) Types.Input.LightRight] = true;

                if (_player.GetAxis("Crouch") < GameSettings.Instance.crouchThreshold)
                    RealTimeInputs[(int) Types.Input.Down] = true;
                else if (_player.GetAxis("Crouch") > GameSettings.Instance.upThreshold)
                    RealTimeInputs[(int) Types.Input.Up] = true;
            }

            if (_player.GetButton("Hop"))
                RealTimeInputs[(int) Types.Input.Jump] = true;
            
            if (_player.GetButtonDown("Neutral"))
                RealTimeInputs[(int) Types.Input.Neutral] = true;
            
            if (_player.GetButton("Strong"))
                RealTimeInputs[(int) Types.Input.Strong] = true;

            if (_player.GetButtonDown("Special"))
                RealTimeInputs[(int) Types.Input.Special] = true;

            if (_player.GetButton("Shield"))
                RealTimeInputs[(int) Types.Input.Shield] = true;

            if (_player.GetButtonDown("Grab"))
                RealTimeInputs[(int) Types.Input.Grab] = true;

            PlayerData.DataPacket.MovementStickAngle.x = _player.GetAxis("Move");
            PlayerData.DataPacket.MovementStickAngle.y = _player.GetAxis("Crouch");
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance.MatchType == Types.MatchType.OnlineMultiplayer)
            {
                if (P2PHandler.Instance.LatencyCalculated)
                {
                    if (_delayedInputSets.Count == P2PHandler.Instance.Delay)
                        ApplyDelayedInputSets();
                    
                    var inputArray = _changedInputs.ToArray();
                    _lastInputSet = new P2PInputSet(inputArray, P2PHandler.Instance.InputPacketsSent);
                    Events.OnInputsChanged(GetComponent<NetworkIdentity>(), inputArray, true);
                    if (_lastInputSet.Inputs.Length > 0)
                    {
                        var temp = $"LOCALINPUT [{_lastInputSet.PacketNumber}] on {P2PHandler.Instance.FrameCounter} {Environment.NewLine}";
                        foreach (var input in _lastInputSet.Inputs)
                        {
                            var state = input.State ? "Pressed" : "Released";
                            temp += $"[{input.InputType}]->{state}{Environment.NewLine}";
                        }
                        Debug.Log(temp);
                    }
                    _delayedInputSets.Add(_lastInputSet);
                    ArchivedInputSets.Add(_lastInputSet);
                }
            }

            _changedInputs.Clear();
        }

        public void ApplyLastInputSet()
        {
            ArchivedInputSets.Add(_lastInputSet);
        }
        
    }
}