﻿/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Org.BouncyCastle.Asn1.X509;

namespace DTLS
{

	  //opaque ASN.1Cert<1..2^24-1>;
	  //struct {
	  //    ASN.1Cert certificate_list<0..2^24-1>;
	  //} Certificate;

	internal class Certificate : IHandshakeMessage
	{

		private List<byte[]> _CertChain;
		private TCertificateType _CertificateType;

		public List<byte[]> CertChain
		{
			get { return _CertChain; }
			set { _CertChain = value; }
		}

		public TCertificateType CertificateType
		{
			get { return _CertificateType; }
			set { _CertificateType = value; }
		}

		public THandshakeType MessageType { get { return THandshakeType.Certificate; } }

		public int CalculateSize(Version version)
		{
			int result = 3; // overall Length
			switch (_CertificateType)
			{
				case TCertificateType.X509:
					if (_CertChain != null)
					{
						foreach (byte[] item in _CertChain)
						{
							result += (3 + item.Length);							
						}
					}
					break;
				case TCertificateType.OpenPGP:
					break;
				case TCertificateType.RawPublicKey:
					break;
				case TCertificateType.Unknown:
					break;
				default:
					break;
			}
			return result;
		}

		public static Certificate Deserialise(Stream stream, TCertificateType certificateType)
		{
			Certificate result = new Certificate();
			int certificateChainLength = NetworkByteOrderConverter.ToInt24(stream);
			if (certificateChainLength > 0)
			{
				result._CertificateType = certificateType;
				if (certificateType == TCertificateType.X509)
				{
					result._CertChain = new List<byte[]>();
					while (certificateChainLength > 0)
					{
						int certificateLength = NetworkByteOrderConverter.ToInt24(stream);
						byte[] certificate = new byte[certificateLength];
						stream.Read(certificate, 0, certificateLength);
						result._CertChain.Add(certificate);
						certificateChainLength = certificateChainLength - certificateLength - 3;
					}
				}
				else
				{
					
				}
			}
			return result;
		}

		public void Serialise(Stream stream, Version version)
		{
			int totalLength = CalculateSize(version);
			NetworkByteOrderConverter.WriteInt24(stream, totalLength-3);
			switch (_CertificateType)
			{
				case TCertificateType.X509:
					if (_CertChain != null)
					{
						foreach (byte[] item in _CertChain)
						{
							NetworkByteOrderConverter.WriteInt24(stream, item.Length);
							stream.Write(item, 0, item.Length);
						}
					}
					break;
				case TCertificateType.OpenPGP:
					break;
				case TCertificateType.RawPublicKey:
					break;
				case TCertificateType.Unknown:
					break;
				default:
					break;
			}
		}




	}
}
