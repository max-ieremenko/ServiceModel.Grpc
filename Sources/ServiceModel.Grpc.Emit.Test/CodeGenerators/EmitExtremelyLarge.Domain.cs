// <copyright>
// Copyright Max Ieremenko
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.ServiceModel;

namespace ServiceModel.Grpc.Emit.CodeGenerators;

#pragma warning disable SA1117 // Parameters should be on same line or separate lines

public partial class EmitExtremelyLarge
{
    [ServiceContract]
    public interface ILarge
    {
        [OperationContract]
        void Test(
            int p1, int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9, int p10, int p11, int p12, int p13, int p14, int p15,
            int p16, int p17, int p18, int p19, int p20, int p21, int p22, int p23, int p24, int p25, int p26, int p27, int p28, int p29,
            int p30, int p31, int p32, int p33, int p34, int p35, int p36, int p37, int p38, int p39, int p40, int p41, int p42, int p43,
            int p44, int p45, int p46, int p47, int p48, int p49, int p50, int p51, int p52, int p53, int p54, int p55, int p56, int p57,
            int p58, int p59, int p60, int p61, int p62, int p63, int p64, int p65, int p66, int p67, int p68, int p69, int p70, int p71,
            int p72, int p73, int p74, int p75, int p76, int p77, int p78, int p79, int p80, int p81, int p82, int p83, int p84, int p85,
            int p86, int p87, int p88, int p89, int p90, int p91, int p92, int p93, int p94, int p95, int p96, int p97, int p98, int p99,
            int p100, int p101, int p102, int p103, int p104, int p105, int p106, int p107, int p108, int p109, int p110, int p111, int p112,
            int p113, int p114, int p115, int p116, int p117, int p118, int p119, int p120, int p121, int p122, int p123, int p124, int p125,
            int p126, int p127, int p128, int p129, int p130, int p131, int p132, int p133, int p134, int p135, int p136, int p137, int p138,
            int p139, int p140, int p141, int p142, int p143, int p144, int p145, int p146, int p147, int p148, int p149, int p150, int p151,
            int p152, int p153, int p154, int p155, int p156, int p157, int p158, int p159, int p160, int p161, int p162, int p163, int p164,
            int p165, int p166, int p167, int p168, int p169, int p170, int p171, int p172, int p173, int p174, int p175, int p176, int p177,
            int p178, int p179, int p180, int p181, int p182, int p183, int p184, int p185, int p186, int p187, int p188, int p189, int p190,
            int p191, int p192, int p193, int p194, int p195, int p196, int p197, int p198, int p199, int p200, int p201, int p202, int p203,
            int p204, int p205, int p206, int p207, int p208, int p209, int p210, int p211, int p212, int p213, int p214, int p215, int p216,
            int p217, int p218, int p219, int p220, int p221, int p222, int p223, int p224, int p225, int p226, int p227, int p228, int p229,
            int p230, int p231, int p232, int p233, int p234, int p235, int p236, int p237, int p238, int p239, int p240, int p241, int p242,
            int p243, int p244, int p245, int p246, int p247, int p248, int p249, int p250, int p251, int p252, int p253, int p254, int p255);
    }
}