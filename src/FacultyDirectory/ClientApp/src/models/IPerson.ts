import { ICorePersonProperties } from "./ICorePersonProperties";
export interface IPerson extends ICorePersonProperties {
    id: number;
    iamId: string;
    kerberos: string;
}
