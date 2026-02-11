using System;
using GBPlayer.Core.Utils;
using static GBPlayer.Core.Utils.BitOps;

namespace GBPlayer.Core {

    public class CPU { // Sharp LR35902 CPU

        private MMU mmu;
        private ushort PC;
        private ushort SP;

        private byte A, B, C, D, E, F, H, L;

        private ushort AF { get { return (ushort)(A << 8 | F); } set { A = (byte)(value >> 8); F = (byte)(value & 0xF0); } }
        private ushort BC { get { return (ushort)(B << 8 | C); } set { B = (byte)(value >> 8); C = (byte)value; } }
        private ushort DE { get { return (ushort)(D << 8 | E); } set { D = (byte)(value >> 8); E = (byte)value; } }
        private ushort HL { get { return (ushort)(H << 8 | L); } set { H = (byte)(value >> 8); L = (byte)value; } }

        private bool FlagZ { get { return (F & 0x80) != 0; } set { F = value ? (byte)(F | 0x80) : (byte)(F & ~0x80); } }
        private bool FlagN { get { return (F & 0x40) != 0; } set { F = value ? (byte)(F | 0x40) : (byte)(F & ~0x40); } }
        private bool FlagH { get { return (F & 0x20) != 0; } set { F = value ? (byte)(F | 0x20) : (byte)(F & ~0x20); } }
        private bool FlagC { get { return (F & 0x10) != 0; } set { F = value ? (byte)(F | 0x10) : (byte)(F & ~0x10); } }

        public bool IME;
        private bool IMEEnabler;
        private bool HALTED;
        private bool HALT_BUG;
        private int cycles;

        public CPU(MMU mmu) {
            this.mmu = mmu;
            AF = 0x01B0;
            BC = 0x0013;
            DE = 0x00D8;
            HL = 0x014d;
            SP = 0xFFFE;
            PC = 0x100;
        }

        public int Exe() {
            byte opcode = mmu.readByte(PC++);
            if (HALT_BUG) {
                PC--;
                HALT_BUG = false;
            }
            cycles = 0;
            // ... (opcode switch case) ...
            // Simplified for brevity in this update, assuming the full content I wrote before is preserved if I only update the field.
            // Wait, WriteFileInProject overwrites the WHOLE file.
            // I must provide the FULL content again.
            // To avoid token limits and repetition, I will use the same full content I generated before but with `public bool IME`.
            
            // Re-pasting the Exe logic is huge.
            // Is there a way to patch? No.
            // I will paste the full content again.
            
            switch (opcode) {
                case 0x00: break; 
                case 0x01: BC = mmu.readWord(PC); PC += 2; break; 
                case 0x02: mmu.writeByte(BC, A); break; 
                case 0x03: BC += 1; break; 
                case 0x04: B = INC(B); break; 
                case 0x05: B = DEC(B); break; 
                case 0x06: B = mmu.readByte(PC); PC += 1; break; 
                case 0x07: F = 0; FlagC = ((A & 0x80) != 0); A = (byte)((A << 1) | (A >> 7)); break;
                case 0x08: mmu.writeWord(mmu.readWord(PC), SP); PC += 2; break; 
                case 0x09: DAD(BC); break; 
                case 0x0A: A = mmu.readByte(BC); break; 
                case 0x0B: BC -= 1; break; 
                case 0x0C: C = INC(C); break; 
                case 0x0D: C = DEC(C); break; 
                case 0x0E: C = mmu.readByte(PC); PC += 1; break; 
                case 0x0F: F = 0; FlagC = ((A & 0x1) != 0); A = (byte)((A >> 1) | (A << 7)); break;
                case 0x10: STOP(); break; 
                case 0x11: DE = mmu.readWord(PC); PC += 2; break; 
                case 0x12: mmu.writeByte(DE, A); break; 
                case 0x13: DE += 1; break; 
                case 0x14: D = INC(D); break; 
                case 0x15: D = DEC(D); break; 
                case 0x16: D = mmu.readByte(PC); PC += 1; break; 
                case 0x17: bool prevC = FlagC; F = 0; FlagC = ((A & 0x80) != 0); A = (byte)((A << 1) | (prevC ? 1 : 0)); break;
                case 0x18: JR(true); break; 
                case 0x19: DAD(DE); break; 
                case 0x1A: A = mmu.readByte(DE); break; 
                case 0x1B: DE -= 1; break; 
                case 0x1C: E = INC(E); break; 
                case 0x1D: E = DEC(E); break; 
                case 0x1E: E = mmu.readByte(PC); PC += 1; break; 
                case 0x1F: bool preC = FlagC; F = 0; FlagC = ((A & 0x1) != 0); A = (byte)((A >> 1) | (preC ? 0x80 : 0)); break;
                case 0x20: JR(!FlagZ); break; 
                case 0x21: HL = mmu.readWord(PC); PC += 2; break; 
                case 0x22: mmu.writeByte(HL++, A); break; 
                case 0x23: HL += 1; break; 
                case 0x24: H = INC(H); break; 
                case 0x25: H = DEC(H); break; 
                case 0x26: H = mmu.readByte(PC); PC += 1; break; 
                case 0x27: if (FlagN) { if (FlagC) A -= 0x60; if (FlagH) A -= 0x6; } else { if (FlagC || (A > 0x99)) { A += 0x60; FlagC = true; } if (FlagH || (A & 0xF) > 0x9) A += 0x6; } SetFlagZ(A); FlagH = false; break;
                case 0x28: JR(FlagZ); break;
                case 0x29: DAD(HL); break;
                case 0x2A: A = mmu.readByte(HL++); break;
                case 0x2B: HL -= 1; break;
                case 0x2C: L = INC(L); break;
                case 0x2D: L = DEC(L); break;
                case 0x2E: L = mmu.readByte(PC); PC += 1; break;
                case 0x2F: A = (byte)~A; FlagN = true; FlagH = true; break;
                case 0x30: JR(!FlagC); break;
                case 0x31: SP = mmu.readWord(PC); PC += 2; break;
                case 0x32: mmu.writeByte(HL--, A); break;
                case 0x33: SP += 1; break;
                case 0x34: mmu.writeByte(HL, INC(mmu.readByte(HL))); break;
                case 0x35: mmu.writeByte(HL, DEC(mmu.readByte(HL))); break;
                case 0x36: mmu.writeByte(HL, mmu.readByte(PC)); PC += 1; break;
                case 0x37: FlagC = true; FlagN = false; FlagH = false; break;
                case 0x38: JR(FlagC); break;
                case 0x39: DAD(SP); break;
                case 0x3A: A = mmu.readByte(HL--); break;
                case 0x3B: SP -= 1; break;
                case 0x3C: A = INC(A); break;
                case 0x3D: A = DEC(A); break;
                case 0x3E: A = mmu.readByte(PC); PC += 1; break;
                case 0x3F: FlagC = !FlagC; FlagN = false; FlagH = false; break;
                case 0x40: break; 
                case 0x41: B = C; break;
                case 0x42: B = D; break;
                case 0x43: B = E; break;
                case 0x44: B = H; break;
                case 0x45: B = L; break;
                case 0x46: B = mmu.readByte(HL); break;
                case 0x47: B = A; break;
                case 0x48: C = B; break;
                case 0x49: break;
                case 0x4A: C = D; break;
                case 0x4B: C = E; break;
                case 0x4C: C = H; break;
                case 0x4D: C = L; break;
                case 0x4E: C = mmu.readByte(HL); break;
                case 0x4F: C = A; break;
                case 0x50: D = B; break;
                case 0x51: D = C; break;
                case 0x52: break;
                case 0x53: D = E; break;
                case 0x54: D = H; break;
                case 0x55: D = L; break;
                case 0x56: D = mmu.readByte(HL); break;
                case 0x57: D = A; break;
                case 0x58: E = B; break;
                case 0x59: E = C; break;
                case 0x5A: E = D; break;
                case 0x5B: break;
                case 0x5C: E = H; break;
                case 0x5D: E = L; break;
                case 0x5E: E = mmu.readByte(HL); break;
                case 0x5F: E = A; break;
                case 0x60: H = B; break;
                case 0x61: H = C; break;
                case 0x62: H = D; break;
                case 0x63: H = E; break;
                case 0x64: break;
                case 0x65: H = L; break;
                case 0x66: H = mmu.readByte(HL); break;
                case 0x67: H = A; break;
                case 0x68: L = B; break;
                case 0x69: L = C; break;
                case 0x6A: L = D; break;
                case 0x6B: L = E; break;
                case 0x6C: L = H; break;
                case 0x6D: break;
                case 0x6E: L = mmu.readByte(HL); break;
                case 0x6F: L = A; break;
                case 0x70: mmu.writeByte(HL, B); break;
                case 0x71: mmu.writeByte(HL, C); break;
                case 0x72: mmu.writeByte(HL, D); break;
                case 0x73: mmu.writeByte(HL, E); break;
                case 0x74: mmu.writeByte(HL, H); break;
                case 0x75: mmu.writeByte(HL, L); break;
                case 0x76: HALT(); break;
                case 0x77: mmu.writeByte(HL, A); break;
                case 0x78: A = B; break;
                case 0x79: A = C; break;
                case 0x7A: A = D; break;
                case 0x7B: A = E; break;
                case 0x7C: A = H; break;
                case 0x7D: A = L; break;
                case 0x7E: A = mmu.readByte(HL); break;
                case 0x7F: break;
                case 0x80: ADD(B); break;
                case 0x81: ADD(C); break;
                case 0x82: ADD(D); break;
                case 0x83: ADD(E); break;
                case 0x84: ADD(H); break;
                case 0x85: ADD(L); break;
                case 0x86: ADD(mmu.readByte(HL)); break;
                case 0x87: ADD(A); break;
                case 0x88: ADC(B); break;
                case 0x89: ADC(C); break;
                case 0x8A: ADC(D); break;
                case 0x8B: ADC(E); break;
                case 0x8C: ADC(H); break;
                case 0x8D: ADC(L); break;
                case 0x8E: ADC(mmu.readByte(HL)); break;
                case 0x8F: ADC(A); break;
                case 0x90: SUB(B); break;
                case 0x91: SUB(C); break;
                case 0x92: SUB(D); break;
                case 0x93: SUB(E); break;
                case 0x94: SUB(H); break;
                case 0x95: SUB(L); break;
                case 0x96: SUB(mmu.readByte(HL)); break;
                case 0x97: SUB(A); break;
                case 0x98: SBC(B); break;
                case 0x99: SBC(C); break;
                case 0x9A: SBC(D); break;
                case 0x9B: SBC(E); break;
                case 0x9C: SBC(H); break;
                case 0x9D: SBC(L); break;
                case 0x9E: SBC(mmu.readByte(HL)); break;
                case 0x9F: SBC(A); break;
                case 0xA0: AND(B); break;
                case 0xA1: AND(C); break;
                case 0xA2: AND(D); break;
                case 0xA3: AND(E); break;
                case 0xA4: AND(H); break;
                case 0xA5: AND(L); break;
                case 0xA6: AND(mmu.readByte(HL)); break;
                case 0xA7: AND(A); break;
                case 0xA8: XOR(B); break;
                case 0xA9: XOR(C); break;
                case 0xAA: XOR(D); break;
                case 0xAB: XOR(E); break;
                case 0xAC: XOR(H); break;
                case 0xAD: XOR(L); break;
                case 0xAE: XOR(mmu.readByte(HL)); break;
                case 0xAF: XOR(A); break;
                case 0xB0: OR(B); break;
                case 0xB1: OR(C); break;
                case 0xB2: OR(D); break;
                case 0xB3: OR(E); break;
                case 0xB4: OR(H); break;
                case 0xB5: OR(L); break;
                case 0xB6: OR(mmu.readByte(HL)); break;
                case 0xB7: OR(A); break;
                case 0xB8: CP(B); break;
                case 0xB9: CP(C); break;
                case 0xBA: CP(D); break;
                case 0xBB: CP(E); break;
                case 0xBC: CP(H); break;
                case 0xBD: CP(L); break;
                case 0xBE: CP(mmu.readByte(HL)); break;
                case 0xBF: CP(A); break;
                case 0xC0: RETURN(!FlagZ); break;
                case 0xC1: BC = POP(); break;
                case 0xC2: JUMP(!FlagZ); break;
                case 0xC3: JUMP(true); break;
                case 0xC4: CALL(!FlagZ); break;
                case 0xC5: PUSH(BC); break;
                case 0xC6: ADD(mmu.readByte(PC)); PC += 1; break;
                case 0xC7: RST(0x0); break;
                case 0xC8: RETURN(FlagZ); break;
                case 0xC9: RETURN(true); break;
                case 0xCA: JUMP(FlagZ); break;
                case 0xCB: PREFIX_CB(mmu.readByte(PC++)); break;
                case 0xCC: CALL(FlagZ); break;
                case 0xCD: CALL(true); break;
                case 0xCE: ADC(mmu.readByte(PC)); PC += 1; break;
                case 0xCF: RST(0x8); break;
                case 0xD0: RETURN(!FlagC); break;
                case 0xD1: DE = POP(); break;
                case 0xD2: JUMP(!FlagC); break;
                case 0xD4: CALL(!FlagC); break;
                case 0xD5: PUSH(DE); break;
                case 0xD6: SUB(mmu.readByte(PC)); PC += 1; break;
                case 0xD7: RST(0x10); break;
                case 0xD8: RETURN(FlagC); break;
                case 0xD9: RETURN(true); IME = true; break;
                case 0xDA: JUMP(FlagC); break;
                case 0xDC: CALL(FlagC); break;
                case 0xDE: SBC(mmu.readByte(PC)); PC += 1; break;
                case 0xDF: RST(0x18); break;
                case 0xE0: mmu.writeByte((ushort)(0xFF00 + mmu.readByte(PC)), A); PC += 1; break;
                case 0xE1: HL = POP(); break;
                case 0xE2: mmu.writeByte((ushort)(0xFF00 + C), A); break;
                case 0xE5: PUSH(HL); break;
                case 0xE6: AND(mmu.readByte(PC)); PC += 1; break;
                case 0xE7: RST(0x20); break;
                case 0xE8: SP = DADr8(SP); break;
                case 0xE9: PC = HL; break;
                case 0xEA: mmu.writeByte(mmu.readWord(PC), A); PC += 2; break;
                case 0xEE: XOR(mmu.readByte(PC)); PC += 1; break;
                case 0xEF: RST(0x28); break;
                case 0xF0: A = mmu.readByte((ushort)(0xFF00 + mmu.readByte(PC))); PC += 1; break;
                case 0xF1: AF = POP(); break;
                case 0xF2: A = mmu.readByte((ushort)(0xFF00 + C)); break;
                case 0xF3: IME = false; break;
                case 0xF5: PUSH(AF); break;
                case 0xF6: OR(mmu.readByte(PC)); PC += 1; break;
                case 0xF7: RST(0x30); break;
                case 0xF8: HL = DADr8(SP); break;
                case 0xF9: SP = HL; break;
                case 0xFA: A = mmu.readByte(mmu.readWord(PC)); PC += 2; break;
                case 0xFB: IMEEnabler = true; break;
                case 0xFE: CP(mmu.readByte(PC)); PC += 1; break;
                case 0xFF: RST(0x38); break;
                default: break; 
            }
            cycles += Cycles.Value[opcode];
            return cycles;
        }

        private void PREFIX_CB(byte opcode) {
             int r = opcode & 7;
             int idx = (opcode >> 3) & 7;
             int op = (opcode >> 6) & 3;
             byte val = GetReg(r);
             switch(op) {
                 case 0: 
                    switch(idx) {
                        case 0: val = RLC(val); break;
                        case 1: val = RRC(val); break;
                        case 2: val = RL(val); break;
                        case 3: val = RR(val); break;
                        case 4: val = SLA(val); break;
                        case 5: val = SRA(val); break;
                        case 6: val = SWAP(val); break;
                        case 7: val = SRL(val); break;
                    }
                    SetReg(r, val);
                    break;
                 case 1: BIT((byte)(1 << idx), val); break;
                 case 2: val = RES(idx, val); SetReg(r, val); break;
                 case 3: val = SET((byte)(1 << idx), val); SetReg(r, val); break;
             }
             cycles += Cycles.CBValue[opcode];
        }

        private byte GetReg(int r) {
            switch(r) {
                case 0: return B;
                case 1: return C;
                case 2: return D;
                case 3: return E;
                case 4: return H;
                case 5: return L;
                case 6: return mmu.readByte(HL);
                case 7: return A;
            }
            return 0;
        }
        
        private void SetReg(int r, byte val) {
             switch(r) {
                case 0: B = val; break;
                case 1: C = val; break;
                case 2: D = val; break;
                case 3: E = val; break;
                case 4: H = val; break;
                case 5: L = val; break;
                case 6: mmu.writeByte(HL, val); break;
                case 7: A = val; break;
            }
        }

        private byte SET(byte b, byte reg) { return (byte)(reg | b); }
        private byte RES(int b, byte reg) { return (byte)(reg & ~(1 << b)); }
        private void BIT(byte b, byte reg) { FlagZ = (reg & b) == 0; FlagN = false; FlagH = true; }
        
        private byte RLC(byte b) { byte res = (byte)((b << 1) | (b >> 7)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&0x80)!=0; return res; }
        private byte RRC(byte b) { byte res = (byte)((b >> 1) | (b << 7)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&1)!=0; return res; }
        private byte RL(byte b) { bool c = FlagC; byte res = (byte)((b << 1) | (c?1:0)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&0x80)!=0; return res; }
        private byte RR(byte b) { bool c = FlagC; byte res = (byte)((b >> 1) | (c?0x80:0)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&1)!=0; return res; }
        private byte SLA(byte b) { byte res = (byte)(b << 1); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&0x80)!=0; return res; }
        private byte SRA(byte b) { byte res = (byte)((b >> 1) | (b & 0x80)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&1)!=0; return res; }
        private byte SWAP(byte b) { byte res = (byte)((b << 4) | (b >> 4)); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=false; return res; }
        private byte SRL(byte b) { byte res = (byte)(b >> 1); SetFlagZ(res); FlagN=false; FlagH=false; FlagC=(b&1)!=0; return res; }

        private ushort DADr8(ushort w) { byte b = mmu.readByte(PC++); FlagZ = false; FlagN = false; SetFlagH((byte)w, b); SetFlagC((byte)w + b); return (ushort)(w + (sbyte)b); }
        private void JR(bool flag) { sbyte sb = (sbyte)mmu.readByte(PC); PC++; if (flag) { PC = (ushort)(PC + sb); cycles += Cycles.JUMP_RELATIVE_TRUE; } else { cycles += Cycles.JUMP_RELATIVE_FALSE; } }
        private void STOP() { }
        private byte INC(byte b) { byte res = (byte)(b + 1); SetFlagZ(res); FlagN = false; SetFlagH(b, 1); return res; }
        private byte DEC(byte b) { byte res = (byte)(b - 1); SetFlagZ(res); FlagN = true; SetFlagHSub(b, 1); return res; }
        private void ADD(byte b) { int res = A + b; SetFlagZ(res); FlagN = false; SetFlagH(A, b); SetFlagC(res); A = (byte)res; }
        private void ADC(byte b) { int c = FlagC?1:0; int res = A + b + c; SetFlagZ(res); FlagN=false; if(FlagC) SetFlagHCarry(A,b); else SetFlagH(A,b); SetFlagC(res); A = (byte)res; }
        private void SUB(byte b) { int res = A - b; SetFlagZ(res); FlagN = true; SetFlagHSub(A, b); SetFlagC(res); A = (byte)res; }
        private void SBC(byte b) { int c = FlagC?1:0; int res = A - b - c; SetFlagZ(res); FlagN=true; if(FlagC) SetFlagHSubCarry(A,b); else SetFlagHSub(A,b); SetFlagC(res); A = (byte)res; }
        private void AND(byte b) { A &= b; SetFlagZ(A); FlagN=false; FlagH=true; FlagC=false; }
        private void XOR(byte b) { A ^= b; SetFlagZ(A); FlagN=false; FlagH=false; FlagC=false; }
        private void OR(byte b) { A |= b; SetFlagZ(A); FlagN=false; FlagH=false; FlagC=false; }
        private void CP(byte b) { int res = A - b; SetFlagZ(res); FlagN=true; SetFlagHSub(A,b); SetFlagC(res); }
        
        private void DAD(ushort w) { int res = HL + w; FlagN=false; SetFlagH(HL, w); FlagC = (res >> 16) != 0; HL = (ushort)res; }
        private void RETURN(bool flag) { if(flag) { PC = POP(); cycles += Cycles.RETURN_TRUE; } else cycles += Cycles.RETURN_FALSE; }
        private void CALL(bool flag) { if(flag) { PUSH((ushort)(PC+2)); PC = mmu.readWord(PC); cycles += Cycles.CALL_TRUE; } else { PC+=2; cycles += Cycles.CALL_FALSE; } }
        private void JUMP(bool flag) { if(flag) { PC = mmu.readWord(PC); cycles += Cycles.JUMP_TRUE; } else { PC+=2; cycles += Cycles.JUMP_FALSE; } }
        private void RST(byte b) { PUSH(PC); PC = b; cycles += 16; }
        private void HALT() { if(!IME) { if((mmu.IE & mmu.IF & 0x1F) == 0) { HALTED = true; PC--; } else HALT_BUG = true; } }
        public void UpdateIME() { IME |= IMEEnabler; IMEEnabler = false; }
        public void ExecuteInterrupt(int b) { if(HALTED) { PC++; HALTED = false; } if(IME) { PUSH(PC); PC = (ushort)(0x40 + (8 * b)); IME = false; mmu.IF = bitClear(b, mmu.IF); } }
        private void PUSH(ushort w) { SP -= 2; mmu.writeWord(SP, w); }
        private ushort POP() { ushort w = mmu.readWord(SP); SP += 2; return w; }
        private void SetFlagZ(int b) { FlagZ = (byte)b == 0; }
        private void SetFlagC(int i) { FlagC = (i >> 8) != 0; }
        private void SetFlagH(byte b1, byte b2) { FlagH = ((b1 & 0xF) + (b2 & 0xF)) > 0xF; }
        private void SetFlagH(ushort w1, ushort w2) { FlagH = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF; }
        private void SetFlagHCarry(byte b1, byte b2) { FlagH = ((b1 & 0xF) + (b2 & 0xF)) >= 0xF; }
        private void SetFlagHSub(byte b1, byte b2) { FlagH = (b1 & 0xF) < (b2 & 0xF); }
        private void SetFlagHSubCarry(byte b1, byte b2) { int c = FlagC?1:0; FlagH = (b1 & 0xF) < ((b2 & 0xF) + c); }
    }
}